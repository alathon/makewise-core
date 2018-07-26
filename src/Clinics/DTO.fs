namespace Clinics

open Domain
open Result
open System

module Dto =
    open EmailAddress
    open WrappedString

    type Name = {
        FirstName: string
        Initial: string
        LastName: string
    } with 
        static member ToDomain (dto:Name) :Result<Domain.Name,string list> = 
            result {
                let! first = String50.create dto.FirstName
                let! last = String50.create dto.LastName
                let initial = String1.create dto.Initial |> String1.asOption
                return { FirstName = first; Initial = initial; LastName = last}
            }
        static member FromDomain (name:Domain.Name) :Name =
            {
                FirstName = name.FirstName |> String50.value
                Initial = name.Initial |> Option.map String1.value |> defaultArg <| ""
                LastName = name.LastName |> String50.value
            }

    type Address = {
        Line1: string
        Line2: string
    } with
        static member ToDomain (dto:Address) :Result<Domain.Address,string list> =
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
        static member FromDomain (addr:Domain.Address) :Address =
            {
                Line1 = String50.value addr.Line1
                Line2 = addr.Line2 |> Option.map String50.value |> defaultArg <| ""
            }

    type EmailAddress = {
        EmailAddress: string
        IsVerified: bool
    } with
        static member FromDomain (email:Domain.EmailAddress.T) :EmailAddress =
            match email with
                | VerifiedEmail e -> { IsVerified = true; EmailAddress = e }
                | UnverifiedEmail e -> { IsVerified = false; EmailAddress = e}
                
        static member ToDomain (dto:EmailAddress) :Result<Domain.EmailAddress.T,string list> = result {
            return match dto with
                    | { IsVerified = true; EmailAddress = e} -> e |> Domain.EmailAddress.create |> Domain.EmailAddress.verify
                    | { IsVerified = false; EmailAddress = e} -> e |> Domain.EmailAddress.create
        }

    type ContactDetails = {
        Address: Address
        Phone: string
        Email: EmailAddress
    }

    type PatientRole = {
        Tag: string // e.g., PaceMaker | Transplant
        DeviceData: DeviceData // Data in case of PaceMaker role
    } with
        static member ToDomain (dto:PatientRole) :Result<Domain.PatientRole,string list> =
            match (dto.Tag, dto.DeviceData) with
            | ("PaceMaker", data) -> Ok (PatientRole.PaceMakerRole data)
            | ("Transplant", _) -> Ok PatientRole.TransplantRole
            | _ -> Error ["Invalid patient role"]
        
        static member FromDomain (role:Domain.PatientRole) :PatientRole =
            match role with
            | PaceMakerRole data -> 
                { 
                    Tag = "PaceMaker"
                    DeviceData = data 
                }
            | TransplantRole -> 
                { 
                    Tag = "Transplant"
                    DeviceData = Unchecked.defaultof<DeviceData>
                }

    type Patient = {
        PrimaryRole: PatientRole
        SecondaryRoles: PatientRole[]
        CprNumber: string
        Name: Name
        ContactDetails: ContactDetails
        CreatedAt: DateTime
    } with
    static member FromDomain (patient:Domain.Patient) :Patient =
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
            CprNumber = patient.CprNumber |> CprNumber.value
            CreatedAt = patient.CreatedAt
        }

    static member ToDomain (dto:Patient) :Result<Domain.Patient,string list> = result {
            let! name = Name.ToDomain dto.Name
            let! addr = Address.ToDomain dto.ContactDetails.Address
            let! email = EmailAddress.ToDomain dto.ContactDetails.Email

            let contactDetails:Domain.ContactDetails = {
                Address = addr
                Phone = Some dto.ContactDetails.Phone |> Option.filter System.String.IsNullOrWhiteSpace
                Email = email
            }
            
            let! primaryRole = PatientRole.ToDomain dto.PrimaryRole
            let! secondaryRoles = dto.SecondaryRoles |> Array.toList |> Result.traverseA PatientRole.ToDomain
            let! cprNumber = CprNumber.make dto.CprNumber

            return {
                Name = name
                ContactDetails = contactDetails
                PrimaryRole = primaryRole
                SecondaryRoles = secondaryRoles
                CprNumber = cprNumber
                CreatedAt = dto.CreatedAt
            }
    }