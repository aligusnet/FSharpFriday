open System
open FSharp.Data
open Microsoft.EntityFrameworkCore

type FsFridayViewers = CsvProvider<"data/2018-06-08.csv">

let connString = "Data Source=streamdata.db"

[<EntryPoint>]
let main argv =

    let dbOptions = new DbContextOptionsBuilder()
    dbOptions.UseSqlite(connString) |> ignore
    use dbContext = new Data.ViewerData(dbOptions.Options)
    dbContext.Database.EnsureCreated() |> ignore

    let data = FsFridayViewers.Load ("data/2018-06-08.csv")

    for row in data.Rows do
        let thisDate = DateTime.Parse("2018-06-08").Add(row.Timestamp.TimeOfDay)
        (thisDate, row.Viewers) ||> printfn "At %O we have %M viewers"
        let newRecord = {
            Data.Id = 0
            Data.MeasurementTime = thisDate
            Data.Viewers = row.Viewers
            Data.NewFollowers = row.``New Followers``.GetValueOrDefault()
            Data.Chatters = row.Chatters
        }
        dbContext.StreamViews.Add newRecord |> ignore
    dbContext.SaveChanges() |> printfn "Loaded records: %d"

    0 // return an integer exit code
