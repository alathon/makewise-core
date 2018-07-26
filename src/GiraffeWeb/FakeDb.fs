namespace Web

open Clinics.Dto
open Clinics.SharedTypes
open System

module private Fake =
    let makePatient :Patient =
        let name = { FirstName = "Joe"; Initial = "G"; LastName = "Schmoe" }
        let address = { 
            Line1 = "Frederikssundsvej 94F, 1.TH"
            Line2 = null 
        }
        let email = { IsVerified = true; EmailAddress = "martin@itsolveonline.net" }
        let phone = "+45 31424342"
        let contactDetails = { Address = address; Phone = phone; Email = email }
        let deviceData = {
                ActivatedAt = DateTime.Now
                DateOfLastShock = None
                FirstImplantation = DateTime.Now
        }

        let primaryRole:PatientRole = {
            Tag = "PaceMaker"
            DeviceData = deviceData
        }

        let secondaryRoles = [| { Tag = "Transplant"; DeviceData = Unchecked.defaultof<DeviceData>; } |]

        let cprNumber = "1212121234"
        
        let createdAt = DateTime.UtcNow

        {
            Name = name
            ContactDetails = contactDetails
            PrimaryRole = primaryRole
            SecondaryRoles = secondaryRoles
            CprNumber = cprNumber
            CreatedAt = createdAt
        }

        

module Result =
    let ofOption err = function
        | Some x -> Ok x
        | None -> Error err

module Db =
    module Patients =
        let mutable private patients :Clinics.Dto.Patient[] = seq { for _ in 0 .. 9 do yield Fake.makePatient } |> Seq.toArray

        let setById id patient :Result<Patient,string list> =
            if id <= Array.length patients-1 && id >= 0
                then patients.[id] <- patient; Ok patient
                else Error ["No such patient"]

        let getById id :Result<Patient,string list> = 
            if id <= Array.length patients-1 && id >= 0
                then Ok patients.[id]
                else Error ["No such patient"]