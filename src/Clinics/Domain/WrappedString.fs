namespace Clinics.Domain

open System

module WrappedString = 
    type String1 = private String1 of string

    module String1 =
        let create str =
            if String.IsNullOrEmpty(str) || String.length str > 1 then
                Error ["Empty or too-long string"]
            else
                Ok (String1 str)

        let value (String1 str) = str

        let asOption res =
            match res with
            | Ok str -> Some str
            | Error _ -> None

    type String50 = private String50 of string
    module String50 =
        let create str =
            if String.IsNullOrEmpty(str) || String.length str >= 50 then
                Error ["Empty or too long string"]
            else
                Ok (String50 str)

        let value (String50 str) = str

        let asOption res =
            match res with
            | Ok str -> Some str
            | Error _ -> None