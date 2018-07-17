module GithubCommitProcessor.Data.Metric

open System
open System.ComponentModel.DataAnnotations
open Microsoft.EntityFrameworkCore

[<CLIMutable>]
type Metric = {
    [<Key>]
    Id : int
    CommitId : string
    Repository : string
    DateStamp : DateTime
    Name : string
    NumFilesChanged : int
}

type GitHubData (options) =
    inherit DbContext (options)
    [<DefaultValue>] val mutable metrics : DbSet<Metric>
    member __.Metrics with get() = __.metrics and set v = __.metrics <- v
