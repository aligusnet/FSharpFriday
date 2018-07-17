namespace GithubCommitProcessor.Data

open System
open System.IO
open FSharp.Data
open Microsoft.EntityFrameworkCore

open Metric


module App =
    [<Literal>]
    let PushSample = __SOURCE_DIRECTORY__ + "/data/push.json"
    type PushWebHook = JsonProvider<PushSample>

    let loadGithubData pushData =
        let connString = "Data Source=pushdata.db"
        let dbOptions = new DbContextOptionsBuilder()
        dbOptions.UseSqlite(connString) |> ignore
        use dbContext = new GitHubData(dbOptions.Options)
        dbContext.Database.EnsureCreated() |> ignore

        let webhook = PushWebHook.Parse pushData
        let dateStamp = DateTime.UtcNow

        let createRecord (commit:PushWebHook.Commit) ={
            Id = 0
            CommitId = webhook.After
            Repository = webhook.Repository.Name
            DateStamp = dateStamp
            Name = commit.Author.Username
            NumFilesChanged = commit.Added.Length + commit.Removed.Length + commit.Modified.Length
        }

        webhook.Commits |> Seq.map (createRecord >> dbContext.Metrics.Add) |> Seq.length |> printfn "Loaded %d records"

        dbContext.SaveChanges() |> printfn "Saved %d records"


    [<EntryPoint>]
    let main argv =
        let filename = if Array.length argv > 0 then argv.[0] else PushSample
        let data = File.ReadAllText filename
        loadGithubData data
        0



