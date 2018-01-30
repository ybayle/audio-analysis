﻿namespace FELT.Results

    open System.Reflection
    open OfficeOpenXml
    open System
    open MQUTeR.FSharp.Shared
    open MQUTeR.FSharp.Shared.Maths
    open Microsoft.FSharp.Collections
    open Microsoft.FSharp.Core
    open System.IO
    open FELT.Classifiers
    open MQUTeR.FSharp.Shared.IO

    type ReportConfig = 
        {
            RunDate : DateTime
            AnalysisType : string
            TestDataBytes: int64
            TrainingDataBytes: int64
            TestOriginalCount: int
            TrainingOriginalCount: int
            ReportDestination: FileInfo
            ReportTemplate: FileInfo
            ExportFrd : bool
            ExportFrn : bool
            
        }

    type Place = int
        
    [<AutoOpen>]
    module EpPlusHelpers = 
        let notInfinity (x:obj) = 
            if x :? double then
                let y = x :?> double
                if System.Double.IsPositiveInfinity(y) then
                    box("Infinity")
                elif System.Double.IsNegativeInfinity(y) then 
                    box("-Infinity")
                else
                    x
            else
                x
        let noInfinities = Seq.map notInfinity
        
        let nullToString (s:obj) =
            if isNull s || String.IsNullOrWhiteSpace(unbox s) then
                ""
            else
                s.ToString()
        let nullToFloat (f:obj) =
            if isNull f || (not (f :? float)) then
                0.0
            else
                f :?> float

        let checkLength (values: seq<_>) =
            let l = Seq.length values
            let m = ExcelPackage.MaxRows - 1000
            if l > m then
                Warnf "Input sequence for writing to spreadsheet is too long, truncating output from %i to %i" l m
                Seq.take m values
            else
                values

        let checkColumns (values: seq<'d>) =
            let l = Seq.length values
            let m = ExcelPackage.MaxColumns - 100
            if l > m then
                Warnf "Input sequence for writing to spreadsheet is too long, truncating output from %i to %i" l m
                Seq.take m values
            else
                values

        let checkColumnUpper j =
            j <= (ExcelPackage.MaxColumns - 100)


        let setv (ws:ExcelWorksheet) name value =
                ws.SetValue(ws.Workbook.Names.[name].Address, value)

        let setvh (ws:ExcelWorksheet) name value hyperlink = 
            let addr = ws.Workbook.Names.[name].Address
            ws.Cells.[addr].Hyperlink <- hyperlink
            ws.SetValue(addr, value)

        let setHorz (ws:ExcelWorksheet) name values f =
            let fdc = ws.Workbook.Names.[name].Start
            values |>
            checkColumns |>
            Seq.iteri (fun index op -> ws.SetValue(fdc.Row, fdc.Column + index, f op))

        let setVert (ws:ExcelWorksheet) name values f =
            let fdc = ws.Workbook.Names.[name].Start
            values |>
            checkLength |>
            Seq.iteri (fun index op -> ws.SetValue(fdc.Row + index, fdc.Column, f op))

        let setCellHorz (ws:ExcelWorksheet) name values f =
            let fdc = ws.Workbook.Names.[name].Start
            values |>
            checkColumns |>
            Seq.iteri (fun index op -> f ws.Cells.[fdc.Row, fdc.Column + index]  op) 

        let setCellVert (ws:ExcelWorksheet) name values f =
            let fdc = ws.Workbook.Names.[name].Start
            values |>
            checkLength |>
            Seq.iteri (fun index op ->f ws.Cells.[fdc.Row + index, fdc.Column] op)

        let setSquare (ws:ExcelWorksheet) name (values: seq<#seq<'d>>) =
            let sc = ws.Workbook.Names.[name].Start
            let colMutex = new MutexSwitch(false)
            // race condition when parallelizing, set out bounds first...
            //ws.Cells.[Seq.length values, Seq.length (Seq.nth 0 values)].Value <- "A value"
//                let vs = PSeq.map (fun row -> row |> Seq.map (fun z -> box z) |> Array.ofSeq) values
//                ws.Cells.[sc.Address].LoadFromArrays vs
            //PSeq.iteri (fun indexi row ->  ws.Cells.[sc.Offset(indexi, 0).Address].LoadFromCollection(row) |> ignore) values
            values |>
            checkLength |>
            Seq.iteri (fun indexi -> Seq.iteri (fun  indexj value ->if checkColumnUpper indexj then ws.SetValue(sc.Row + indexi, sc.Column + indexj, value) else colMutex.Flip() ))
            
            if colMutex.IsFlipped then
                Warnf "The sequence columns exceeded the maximum excel can handle, some columns have been truncated!" 


//            let setSquare2 (ws:ExcelWorksheet) address (values: seq<#seq<'d>>) =
//                let sc = ws.Cells.[address].Start
//                Seq.iteri (fun indexi -> Seq.iteri (fun  indexj value -> ws.SetValue(sc.Row + indexi, sc.Column + indexj, value))) values  

        let setCellSquare (ws:ExcelWorksheet) name (values: seq<#seq<'d>>) (f: ExcelRange -> 'd -> unit) =
            let sc = ws.Workbook.Names.[name].Start
            Seq.iteri (fun indexi -> Seq.iteri (fun  indexj value -> f ws.Cells.[sc.Row + indexi, sc.Column + indexj] value)) values
        
        let fillDown (ws:ExcelWorksheet) (startingRange:ExcelRangeBase) numRows =
            let srCell col = ws.Cells.[startingRange.Start.Row, col]
            let fillRange = startingRange.Offset(1, 0, numRows, startingRange.End.Column - startingRange.Start.Column + 1)
            for col in fillRange.Start.Column..fillRange.End.Column do
                let address = new ExcelAddress(fillRange.Start.Row, col, fillRange.End.Row, col)    
                let src = (srCell col)
                ws.Cells.[address.Address].FormulaR1C1 <- src.FormulaR1C1
              
        let makeEachRowAnArrayFromula (ws:ExcelWorksheet) (fillRange:ExcelRangeBase) =
            for col in fillRange.Start.Column..fillRange.End.Column do
                for row in fillRange.Start.Row..fillRange.End.Row do
                    let address = new ExcelAddress(row, fillRange.Start.Column, row, fillRange.End.Column)    
                    ws.Cells.[row,col].CreateArrayFormula(ws.Cells.[row,col].FormulaR1C1)
     


    type ResultsComputation(config:ReportConfig) = class
        let OnePlace:Place = 1
        

        let countUpFormula = "RC[-1] + 1"
        let version = Assembly.GetAssembly(typeof<ResultsComputation>).GetName() |> (fun x -> sprintf "%s, %s, %s" x.Name (x.Version.ToString()) x.CodeBase)


        static member extractResults (trainingData:Data) classificationResults (testData:Data) =
                let testLength = testData.Classes.Length
                let getClass y = trainingData.Classes.[snd y]
                match classificationResults with
                    | ClassifierResult.Function f ->
                        let g r = (getClass r, fst r)   
                        let h = f >> Array.mapUnzip g
                        Array.Parallel.init2 testLength h

                    | ClassifierResult.ResultArray a ->
                        Array.Parallel.mapJagged getClass a,
                        Array.Parallel.mapJagged fst a
                    | ClassifierResult.ResultSeq s ->
                        let frt : Class[][] = Array.createEmpty testLength
                        let frd : Distance[][] = Array.createEmpty testLength
                        // mutation of above arrays follows
                        s |>
                        PSeq.iteri
                            (fun i row ->
                                let tr, dr  = 
                                    Array.init2 row.Length (fun index -> 
                                                            let cell = row.[index]
                                                            trainingData.Classes.[snd cell],
                                                            fst cell)
                                frt.[int i] <- tr
                                frd.[int i] <- dr
                            )
                        frt, frd
                    | _ -> failwith "Unknown classification result format"

        /// warning this class by default involves a lot of mutation and intrinsically causes side-affects
        member this.Calculate (trainingData:Data) (testData:Data) (classificationResults:ClassifierResult) (opList: (string * string * string) list) =
            
            Log "Result computation start"
            
            //let fullResultsTags = Array.Parallel.mapJagged (fun y -> trainingData.Classes.[snd y]) classificationResults
//
//            Log "full results tags"
//
//            let fullResultsDistances = Array.Parallel.mapJagged fst classificationResults

            let fullResultsTags, fullResultsDistances = 
                ResultsComputation.extractResults trainingData classificationResults testData 



            Log "full results distances and tags completed"

            Log "results cleaned up"

            let opList = List.rev opList

            let features = 
                if testData.Headers = trainingData.Headers then
                    testData.Headers
                else
                    failwith "The headers in the test and training data are not the same. This report format does not support different headers for each data set"

            let uniqueTrainingClasses =
               Array.fold (flip Set.add) Set.empty<Class> trainingData.Classes  
            let uniqueTestClasses =
               Array.fold (flip Set.add) Set.empty<Class> testData.Classes  
            
            Log "uniquness a & b"
            
            let uniqueAll = Set.union uniqueTrainingClasses uniqueTestClasses
            
            Log "uniqueness all"

            let tagsThatOnlyOccurInTestData = (Set.difference uniqueTestClasses uniqueTrainingClasses)
            let tagSummary = 
                [
                    [ config.TrainingOriginalCount ; config.TestOriginalCount; config.TrainingOriginalCount + config.TestOriginalCount ];
                    [trainingData.Classes.Length ; testData.Classes.Length ;  trainingData.Classes.Length + testData.Classes.Length ];
                    [uniqueTrainingClasses.Count; uniqueTestClasses.Count; uniqueAll.Count];
                    [   (Set.difference uniqueTrainingClasses uniqueTestClasses).Count;  
                         tagsThatOnlyOccurInTestData.Count;
                        ((Set.difference uniqueTestClasses uniqueAll).Count + (Set.difference uniqueTrainingClasses uniqueAll).Count) ]
                ]

            Log "tag summary done"



            let placeFunc rowNum class' row =
                match Array.tryFindIndex ((=) class') row with
                    | Some index -> (rowNum, index + 1 )
                    | None -> (rowNum, 0)
            let placing : (RowNumber * Place) array = Array.Parallel.mapi2 placeFunc testData.Classes fullResultsTags
            
            Log "Placing"
            let placeHistogram =  Seq.histogramBy (fun x -> snd x) (seq { 0..trainingData.Classes.Length}) placing
            let numPlaces = Seq.length placeHistogram
            System.Diagnostics.Debug.Assert(trainingData.Classes.Length + 1 = (numPlaces))

            Log "Histogram"


            //let rocBinaryClassifierPlaceLimit = 5
            let rocCurve, modifiedAUC, modifiedCutoffs, sortedRocPoints = 
                // measurements are what we expect, i.e. the "Gold standard". i.e. 100% accuracy.
                // for the library we have to provides hits and misses as numbers
                // IMPORTANT: ROC_Threshold != PLACING. Therefore we have to conduct a ROC curve at each cummlative place we are interested in.
                // AND we have top collapse the cummlative places down to the first place.
                // e.g. rocBinaryClassifierPlaceLimit = 2 -> a | b a c -> 1, a | a b c -> 1, a | c b a -> 0, d | a b c -> 0

            
                // predictions are quantised values of places from the results.
                // i.e. for class A, if it's corresponding hit is in the 4th of 10 places, it's prediction result is 0.6 (closer to a hit)
                // anything that is placed as zero (i.e. not found) should be transformed to zero... i.e. a true negative
                let numPoss = float (numPlaces)
                let increment = (numPoss - 1.0)
                let rocPredictions, rocMeasurements = 
                    let pm input =
                        let testIndex, place = input
                        let m = 
                            if tagsThatOnlyOccurInTestData.Contains testData.Classes.[testIndex] then//|| place > rocBinaryClassifierPlaceLimit then 
                                0.0 
                            else 
                                1.0
                        let p =  
                            if place = 0 then 
                                0.0 
                            else 
                                (numPoss - (float place)) / increment

                        p,m
                        
                    placing |> Array.Parallel.map pm |> Array.unzip
                //File.WriteAllLines("C:\\Temp\\numbers.csv", Seq.map2 (fun a b -> a.ToString() + "," + b.ToString()) rocPredictions rocMeasurements) |> ignore
                let roc = Maths.RocCurve.RocScore rocMeasurements rocPredictions (if numPlaces > 500 then 500 else numPlaces)
                let a, cutoffs, points = Maths.RocCurve.rocCurveToPlacingAUC roc
                roc, a, cutoffs, points

            Log "ROC Curve calculated"

            let placeSummary =
                let places = [|1 ; 5; 10; 25; 50 |]
                let withinPlace p = Map.fold (fun total key count -> if key <= p &&  key > 0 then total + count else total) 0 placeHistogram 
                Array.map (fun place -> [place ; withinPlace place]) places

            Log "pl summary"

            let percentileSummary =
                let percentiles = [|0.01; 0.1; 0.2; 0.25; 0.33; 0.5; 0.66; 0.75; 0.9; 1.0|]
                let percentilesAsPlaces = Array.map (fun x -> x , int( Math.Round(x * double trainingData.Classes.Length))) percentiles
                let numCoveredByPlace p = Map.fold (fun total key count -> if key <= p && key > 0 then total + count else total) 0 placeHistogram 
                Array.map (fun (percentile:float,place) -> [percentile; float (numCoveredByPlace place)]) percentilesAsPlaces
                
            Log "pe summary"
            Log "report creating"
            // create excel package
            let report = new ExcelPackage(config.ReportDestination, config.ReportTemplate)
            let workbook = report.Workbook
            let logws =  workbook.Worksheets.["Log"]
            let sumResults = workbook.Worksheets.["Summary Results"]
            let rocData = workbook.Worksheets.["ROCData"]
            let fullResults = workbook.Worksheets.["Full Results"]
            let fullResultsDist = workbook.Worksheets.["Full Results Distances"]
            
            
            
            let names = workbook.Names
           
            Log "start log sheet"


            // set
            setv logws "RunDate" (config.RunDate.ToString("dddd, dd MMMM yyyy HH:mm:ss"))

            setv logws "Version" (version) 

            setv logws "TrBytes" config.TrainingDataBytes
            setv logws "TeBytes" config.TestDataBytes

            // set op list
            setHorz logws "AlgorithmType" opList fst3
            setHorz logws "AlgorithmName" opList snd3
            setHorz logws "AlgorithmDetails" opList third3
            
            // set feature list
            setHorz logws "FeatureDataTypes" features Map.getValue
            setHorz logws "FeatureNames" features Map.getKey

            // set TagSummary
            setSquare logws "TagSummary" tagSummary

            // results summary
            setSquare logws "PlacementSummary" placeSummary
            names.["PlacementSummary"].Offset(0,2, placeSummary.Length, 1).FormulaR1C1 <- "RC[-1]/Log!Positives"
            names.["PlacementSummary"].Offset(0,3, placeSummary.Length, 1).FormulaR1C1 <- "RC[-2]/(Positives+Negatives)"
            setSquare logws "PercentileSummary" percentileSummary

            Log "end log sheet"



            Log "start summary results sheet data"

            // summary results worksheet
            setSquare sumResults "srPlaces" (placeHistogram |> Map.toSeq |> Seq.map (fun x -> [fst x ; snd x]) |> Seq.sort)
            names.["srPlaces"].Offset(0,2, numPlaces, 1).FormulaR1C1 <- "RC[-1]/Log!TestEndInstanceCount"
            names.["srPlaces"].Offset(1,3, (numPlaces) - 1, 1).FormulaR1C1 <- "SUM(R3C3:RC[-1])"

            Log "end summary results sheet data"

            Log "start roc Data"

            //setv rocData "RocPlaces" rocBinaryClassifierPlaceLimit
            setHorz rocData "RocSummary" [rocCurve.Area; rocCurve.StandardError;  float rocCurve.Positives; float rocCurve.Negatives; float rocCurve.Observations] id
            // setHorz rocData "RocCurveDataHeaders" (RocCurve.PrintRocCurvePoint rocCurve.Points.[0] |> fst) id
            setv rocData "ModifiedAUC" modifiedAUC
            //let sortedRocPoints = (rocCurve.Points |> Seq.zip modifiedCutoffs |> Seq.sortBy (fun x -> (snd x).Cutoff) )
            setVert rocData "ModifiedCutoffs" modifiedCutoffs id
            setSquare rocData "RocCurveData" (Seq.map (RocCurve.PrintRocCurvePoint >> snd >> EpPlusHelpers.noInfinities) sortedRocPoints)

            Log "end roc data "

            if config.ExportFrn then
                Log "Full results"

                // full results
                setVert fullResults "frClasses" testData.Classes id
                setv fullResults "frNumbers" 1

                Log "frn a"

                names.["frNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- countUpFormula
            
                Log "frn b"

                setSquare fullResults "frData" fullResultsTags
                Log "frn c"
                let plc = names.["frPlaces"].Offset(0,0, testData.Classes.Length, 1)
                Log "frn d"
                plc.FormulaR1C1 <- "MATCH(RC[-1],RC[1]:RC[" + trainingData.Classes.Length.ToString() + "],0)"
            else
                Info "Not exporting full results"

            if config.ExportFrd then

                Log "full results distances"

                // full results (distances)
                setVert fullResultsDist "frdClasses" testData.Classes id
                setv fullResultsDist "frdNumbers" 1
            
                Log "frd a"

                names.["frdNumbers"].Offset(0, 1, 1, trainingData.Classes.Length - 1).FormulaR1C1 <- countUpFormula
            
                Log "frd b"

                setSquare fullResultsDist "frdData" fullResultsDistances

                Log "write time taken"
            else
                Info "Not exporting full results distances"


            setv logws "TimeTaken" ((DateTime.Now - config.RunDate).ToString())
            setHorz logws "MemUsage" [ System.Diagnostics.Process.GetCurrentProcess().PeakWorkingSet64; Environment.WorkingSet; System.Diagnostics.Process.GetCurrentProcess().WorkingSet64] id

            Log "write file"

            report.Save()
            report.Dispose()

            Log "done"

            ()
        end