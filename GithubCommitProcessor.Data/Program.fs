namespace GithubCommitProcessor.Data

open System
open System.IO
open FSharp.Data
open Microsoft.EntityFrameworkCore

open Metric


module App =
    [<Literal>]
    let pushSample = __SOURCE_DIRECTORY__ + "/data/push.json"
    type PushWebHook = JsonProvider<pushSample>

    let loadGithubData pushData =
        let connString = "Data Source=pushdata.db"
        let dbOptions = new DbContextOptionsBuilder()
        dbOptions.UseSqlite(connString) |> ignore
        use dbContext = new GitHubData(dbOptions.Options)
        dbContext.Database.EnsureCreated() |> ignore

        let webhook = PushWebHook.Parse(pushData)
        let commitId = webhook.After
        let dateStamp = DateTime.UtcNow
        let repository = webhook.Repository.Name
        for commit in webhook.Commits do
            let author = commit.Author.Username
            let lineCount = commit.Added.Length + commit.Removed.Length + commit.Modified.Length
            let record = {
                Id = 0
                CommitId = commitId
                Repository = repository
                DateStamp = dateStamp
                Name = author
                NumFilesChanged = lineCount
            }
            dbContext.Metrics.Add record |> ignore
        dbContext.SaveChanges() |> printfn "Loaded %d records"


    [<EntryPoint>]
    let main argv =
        let filename = if Array.length argv > 0 then argv.[0] else pushSample
        let data = File.ReadAllText filename
        loadGithubData data
        0



