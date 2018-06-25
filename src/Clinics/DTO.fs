namespace Clinics

open Domain

module Dto =
    type Name = {
        FirstName: string
        Initial: string
        LastName: string
    }

    type Address = {
        Line1: string
        Line2: string
    }

    type Email = {
        Email: string
        IsVerified: bool
    }

    type ContactDetails = {
        Address: Address
        Phone: string
        Email: Email
    }

    type PatientRole = {
        Tag: string // e.g., PaceMaker | Transplant
        DeviceData: DeviceData // Data in case of PaceMaker role
    } 
    
    type PatientRole with 
        static member ToDomain (dto:PatientRole) :Result<Domain.PatientRole,string> =
            match (dto.Tag, dto.DeviceData) with
            | ("PaceMaker", data) -> Ok (Domain.PatientRole.PaceMakerRole data)
            | ("Transplant", _) -> Ok Domain.PatientRole.TransplantRole
            | _ -> Error "Invalid patient role"
        
        static member FromDomain (role:Domain.PatientRole) :PatientRole =
            match role with
            | PaceMakerRole data -> { Tag = "PaceMaker"; DeviceData = data }
            | TransplantRole -> { Tag = "Transplant"; DeviceData = Unchecked.defaultof<DeviceData> }

    type Patient = {
        PrimaryRole: PatientRole
        SecondaryRoles: PatientRole[]
        CprNumber: string
        Name: Name
        ContactDetails: ContactDetails
    }

module Patient =
    open Dto
    open Domain.EmailAddress

    let fromDomain (patient:Domain.Patient) :Dto.Patient =
        let name = {
            FirstName = patient.Name.FirstName |> Domain.String50.value
            Initial = patient.Name.Initial |> Option.map String1.value |> defaultArg <| "" // Map via String1.value or default to ""
            LastName = patient.Name.LastName |> String50.value
        }

        let contactDetails = {
            Address = { 
                Line1 = patient.ContactDetails.Address.Line1
                Line2 = patient.ContactDetails.Address.Line2 |> defaultArg <| "" 
            }
            Phone = patient.ContactDetails.Phone |> defaultArg <| ""
            Email = { IsVerified = match patient.ContactDetails.Email with
                                    | VerifiedEmail _ -> true
                                    | UnverifiedEmail _ -> false
                      Email = match patient.ContactDetails.Email with | e v -> v
            }
        }

        {
            PrimaryRole = patient.PrimaryRole |> PatientRole.FromDomain
            SecondaryRoles = patient.SecondaryRoles |> List.map PatientRole.FromDomain |> List.toArray
            Name = name
            ContactDetails = contactDetails
            CprNumber = patient.CprNumber |> CprNumber.value
        }

    let rec traverseResultA f list =
        // define the applicative functions
        let (<*>) ff r = match r with
                            | Ok a -> Ok (ff a)
                            | Error e -> Error e

        let retn = Result.bind

        let a = retn []

        // define a "cons" function
        let cons head tail = head :: tail

        // loop through the list
        match list with
        | [] -> 
            // if empty, lift [] to a Result
            retn []
        | head::tail ->
            retn cons <*> (f head) <*> (traverseResultA f tail)

    let toDomain (dto:Dto.Patient) :Result<Domain.Patient,string> =
        let name = Name.Create dto.Name.FirstName dto.Name.Initial dto.Name.LastName
        let contactDetails:Domain.ContactDetails = {
            Address = {
                Line1 = dto.ContactDetails.Address.Line1
                Line2 = Some dto.ContactDetails.Address.Line2 |> Option.filter System.String.IsNullOrWhiteSpace
            }
            Phone = Some dto.ContactDetails.Phone |> Option.filter System.String.IsNullOrWhiteSpace
            Email = match dto.ContactDetails.Email with
                    | { IsVerified = true; Email = e} -> e |> Domain.EmailAddress.create |> Domain.EmailAddress.verify
                    | { IsVerified = false; Email = e} -> e |> Domain.EmailAddress.create
        }

        let primaryRole = PatientRole.ToDomain dto.PrimaryRole
        let secondaryRoles = dto.SecondaryRoles |> Array.map PatientRole.ToDomain

        // Maybe what we want is to map Result<PatientRole,string>[] to Result<PatientRole[],string[]> ??
        // Yep.. That seems to be it.


        // TODO: Below needs re-writing with above note!
        // This is _super_ ugly. Isn't there a better way to do this?
        // The behavior we want is to _either_ provide a string[] of errors,
        // OR a PatientRole[] of roles.
        let sss (res:Result<PatientRole,string>[]) = 
            let mutable roles:PatientRole[] = [||]
            let mutable errors:string[] = [||]
            res |> Array.map (fun r -> match r with 
                                        | Ok d -> roles = Array.concat [|d|] roles
                                        | Error e -> errors = Array.concat [|e|] errors)
            (roles,errors)

        let (roles,errors) = sss secondaryRoles
