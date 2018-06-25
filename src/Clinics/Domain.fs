namespace Clinics

open System

module Domain = 
    type String1 = private String1 of string

    module String1 =
        let create str =
            if String.IsNullOrEmpty(str) || String.length str > 1 then
                Error "Empty or too-long string"
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
                Error "Empty or too long string"
            else
                Ok (String50 str)

        let value (String50 str) = str

    type CprNumber = private CprNumber of string
    module CprNumber =
        let make s = 
            if System.Text.RegularExpressions.Regex.IsMatch(s,@"\d{10}")
                then Ok (CprNumber s)
                else Error "Invalid CPR"
        
        let value (CprNumber cpr) = cpr

    module EmailAddress =
        type T =
            private
            | VerifiedEmail of string
            | UnverifiedEmail of string
        
        let create = UnverifiedEmail

        let verify = function
            | UnverifiedEmail e -> VerifiedEmail e
            | VerifiedEmail e -> VerifiedEmail e

        let (|VerifiedEmail|UnverifiedEmail|) = function
            | VerifiedEmail e -> VerifiedEmail e
            | UnverifiedEmail e -> UnverifiedEmail e

    type Name = {
        FirstName: String50;
        Initial: String1 option;
        LastName: String50;
    } with static member Create f i l =
            let first = String50.create f
            let last = String50.create l
            let initial = String1.create i |> String1.asOption // Ignore Result error as its optional

            match (first, last) with
            | (Ok fst, Ok lst) -> Some { FirstName = fst; LastName = lst; Initial = initial; }
            | _ -> None

    type Address = {
        Line1: string;
        Line2: string option;
    }

    type ContactDetails = {
        Address: Address;
        Phone: string option;
        Email: EmailAddress.T
    }

    type PatientRole =
        | PaceMakerRole of DeviceData
        | TransplantRole

    type Patient = {
        PrimaryRole: PatientRole;
        SecondaryRoles: PatientRole list;
        CprNumber: CprNumber;
        Name: Name;
        ContactDetails: ContactDetails;
    }