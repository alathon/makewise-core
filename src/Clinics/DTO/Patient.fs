namespace Clinics.Dto

open Clinics
open Clinics.Domain.WrappedString
open Result

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Name = 
    let ToDomain (dto:Name) :Result<Domain.Patient.Name,string list> = 
        result {
            let! first = String50.create dto.FirstName
            let! last = String50.create dto.LastName
            let initial = String1.create dto.Initial |> String1.asOption
            return { FirstName = first; Initial = initial; LastName = last}
        }
    let FromDomain (name:Domain.Patient.Name) :Name =
        {
            FirstName = name.FirstName |> String50.value
            Initial = name.Initial |> Option.map String1.value |> defaultArg <| ""
            LastName = name.LastName |> String50.value
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Address =
    let ToDomain (dto:Address) :Result<Domain.Patient.Address,string list> =
        result {
            let! line1 = String50.create dto.Line1
            let line2 = 
                match String50.create dto.Line2 with
                | Ok l -> Some l
                | Error _ -> None
            return { 
                Line1 = line1
                Line2 = line2
            }
        }
    let FromDomain (addr:Domain.Patient.Address) :Address =
        {
            Line1 = String50.value addr.Line1
            Line2 = addr.Line2 |> Option.map String50.value |> defaultArg <| ""
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module EmailAddress =
    open Domain.Patient.EmailAddress
    let FromDomain (email:Domain.Patient.EmailAddress.T) :EmailAddress =
        match email with
            | VerifiedEmail e -> { IsVerified = true; EmailAddress = e }
            | UnverifiedEmail e -> { IsVerified = false; EmailAddress = e}
            
    let ToDomain (dto:EmailAddress) :Result<Domain.Patient.EmailAddress.T,string list> = result {
        return match dto with
                | { IsVerified = true; EmailAddress = e} -> e |> Domain.Patient.EmailAddress.create |> Domain.Patient.EmailAddress.verify
                | { IsVerified = false; EmailAddress = e} -> e |> Domain.Patient.EmailAddress.create
    }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module PatientRole =
    let ToDomain (dto:PatientRole) :Result<Domain.Patient.PatientRole,string list> =
        match (dto.Tag, dto.DeviceData) with
        | ("PaceMaker", data) -> Ok (Domain.Patient.PatientRole.PaceMakerRole data)
        | ("Transplant", _) -> Ok Domain.Patient.PatientRole.TransplantRole
        | _ -> Error ["Invalid patient role"]
    
    let FromDomain (role:Domain.Patient.PatientRole) :PatientRole =
        match role with
        | Domain.Patient.PaceMakerRole data -> 
            { 
                Tag = "PaceMaker"
                DeviceData = data 
            }
        | Domain.Patient.TransplantRole -> 
            { 
                Tag = "Transplant"
                DeviceData = Unchecked.defaultof<DeviceData>
            }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Patient =
    let FromDomain (patient:Domain.Patient.T) :Patient =
        let name = Name.FromDomain patient.Name
        let contactDetails = {
            Address = Address.FromDomain patient.ContactDetails.Address
            Phone = patient.ContactDetails.Phone |> defaultArg <| ""
            Email = EmailAddress.FromDomain patient.ContactDetails.Email
        }

        {
            PrimaryRole = patient.PrimaryRole |> PatientRole.FromDomain
            SecondaryRoles = patient.SecondaryRoles |> List.map PatientRole.FromDomain |> List.toArray
            Name = name
            ContactDetails = contactDetails
            CprNumber = patient.CprNumber |> Domain.Patient.CprNumber.value
            CreatedAt = patient.CreatedAt
        }

    let ToDomain (dto:Patient) :Result<Domain.Patient.T,string list> = result {
            let! name = Name.ToDomain dto.Name
            let! addr = Address.ToDomain dto.ContactDetails.Address
            let! email = EmailAddress.ToDomain dto.ContactDetails.Email

            let contactDetails:Domain.Patient.ContactDetails = {
                Address = addr
                Phone = Some dto.ContactDetails.Phone |> Option.filter System.String.IsNullOrWhiteSpace
                Email = email
            }
            
            let! primaryRole = PatientRole.ToDomain dto.PrimaryRole
            let! secondaryRoles = dto.SecondaryRoles |> Array.toList |> Result.traverseA PatientRole.ToDomain
            let! cprNumber = Domain.Patient.CprNumber.make dto.CprNumber

            return {
                Name = name
                ContactDetails = contactDetails
                PrimaryRole = primaryRole
                SecondaryRoles = secondaryRoles
                CprNumber = cprNumber
                CreatedAt = dto.CreatedAt
            }
    }