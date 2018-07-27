namespace Clinics.Domain

open System
open Clinics

module Patient = 
    open WrappedString

    type CprNumber = private CprNumber of string
    module CprNumber =
        let make s = 
            if String.IsNullOrEmpty(s) || not(System.Text.RegularExpressions.Regex.IsMatch(s,@"^\d{10}$"))
                then Error ["Invalid CPR"]
                else Ok (CprNumber s)
        
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
    }

    type Address = {
        Line1: String50;
        Line2: String50 option;
    }

    type ContactDetails = {
        Address: Address;
        Phone: string option;
        Email: EmailAddress.T
    }

    type PatientRole =
        | PaceMakerRole of DeviceData
        | TransplantRole

    type T = {
        PrimaryRole: PatientRole;
        SecondaryRoles: PatientRole list;
        CprNumber: CprNumber;
        Name: Name;
        ContactDetails: ContactDetails;
        CreatedAt: DateTime;
    }