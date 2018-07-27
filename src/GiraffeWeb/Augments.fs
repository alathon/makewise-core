namespace Clinics

// The below are slight adaptations of the
// tutorial from fsharpforfunandprofit on
// the Applicative versions of sequence and 
// traverse.
// See e.g., https://fsharpforfunandprofit.com/posts/elevated-world-4/#traverse
// and https://fsharpforfunandprofit.com/posts/elevated-world-4/#sequence
// for more information.
// - Martin 25-06-2018
module Result = 
    type ResultBuilder() =
        member _x.Return(v) = Ok v
        member _x.ReturnFrom(v) = v
        member _x.Bind(v,f) = Result.bind f v

    let result = new ResultBuilder()

    // Lift 'a -> 'b to 'a -> Result<'b,'c>
    let switch f x = 
        f x |> Result.Ok

    let tee f x =
        f x
        x |> Result.Ok

    // Parallel composition with result and error collection using
    // okFn and errorFn respectively
    let private parallelCombine okFn errorFn v1 v2 x =
        match (v1 x, v2 x) with
        | Ok o1, Ok o2 -> Ok (okFn o1 o2)
        | Error e1, Ok _ -> Error e1
        | Ok _, Error e2 -> Error e2
        | Error e1, Error e2 -> Error (errorFn e1 e2)
    
    // Sequential composition with error collection using errorFn
    let private sequentialCombine errorFn v1 v2 i =
        match v1 i with
        | Ok a -> 
            match v2 a with
            | Ok b -> Ok b
            | Error e -> Error e
        | Error e1 -> 
            match v2 i with
            | Ok _ -> Error e1
            | Error e2 -> Error (errorFn e1 e2)

    // Sequential composition with short-circuiting.
    let (>=>) v1 v2 = v1 >> (Result.bind v2)

    let (|||) v1 v2 =
        let okFn r1 _ = r1
        let errorFn e1 e2 = List.concat [e1;e2]
        parallelCombine okFn errorFn v1 v2

    let (&&&) v1 v2 = 
        let errorFn e1 e2 = List.concat [e1; e2]
        sequentialCombine errorFn v1 v2

    /// Map a Result producing function over a list to get a new Result 
    /// using applicative style
    /// ('a -> Result<'b>) -> 'a list -> Result<'b list>
    let traverseA f list =
        // define the applicative functions
        let (<*>) fRes xRes =
            match fRes, xRes with
            | Ok f, Ok x -> Ok (f x)
            | Error e, Ok _ -> Error e
            | Ok _, Error e -> Error e
            | Error e1, Error e2 -> Error (List.concat [e1;e2])

        let cons head tail = head :: tail

        // right fold over the list
        let initState = Result.Ok []
        let folder head tail = 
            Result.Ok cons <*> (f head) <*> tail

        List.foldBack folder list initState 

    let bindOption f xOpt =
        match xOpt with
        | Some x -> f x |> Result.map Some
        | None -> Ok None

    let ofOption err = function
    | Some x -> Ok x
    | None -> Error err

module List =   
    /// Transform a "list<Result>" into a "Result<list>"
    /// and collect the results using apply
    /// Result<'a> list -> Result<'a list>
    let sequenceResultA x = Result.traverseA id x