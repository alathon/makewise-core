namespace Clinics.Domain

open System

module WrappedString = 
    open Result
    type IWrappedString =
        abstract Value : string

    let create canonicalize isValid ctor (fname:string) (s:string) =
        if s = null
        then Error [sprintf "Attempted to create null string for field %s" fname]
        else
            let s' = canonicalize s
            if isValid s'
            then Ok (ctor s')
            else Error [sprintf "Invalid string (%s) when creating field %s" fname s]

    let apply f (s:IWrappedString) =
        s.Value |> f
    let value s = apply id s

    let equals l r = (value l) = (value r)

    let compareTo l r = (value l).CompareTo (value r)

    let lengthValidator len (s:string) = s.Length <= len

    type SafeString = private SafeString of string with
        interface IWrappedString with
            member this.Value = let (SafeString s) = this in s

    type String100 = private String100 of string with
        interface IWrappedString with
            member this.Value = let (String100 s) = this in s
    
    type String1 = private String1 of string with
        interface IWrappedString with
            member this.Value = let (String1 s) = this in s

    let safeStr = create id (fun x -> x |> isNull |> not) SafeString

    let string100 = create id (lengthValidator 100) String100
    let string1 = create id (lengthValidator 1) String1

    let asOption = function
        | Ok ws -> Some ws
        | Error e -> None
    
    let valueOrDefault d v =
        v 
        |> Option.map value 
        |> defaultArg <| d