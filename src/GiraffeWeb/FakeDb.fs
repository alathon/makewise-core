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

// A fake in-memory DB, which just stores the Patient.Dto's.
// Normally you'd be storing the SQLProvider's generated type,
// and so we would normally need an explicit toDomain function
// from the SQLProvider's generated type to the domain type.
// For now we use the Dto's existing ToDomain function for brevity.
module Db =
    module Patients =
        let mutable private patients :Patient[] = seq { for _ in 0 .. 9 do yield Fake.makePatient } |> Seq.toArray

        let setById id patient :Result<unit,string list> =
            if id <= Array.length patients-1 && id >= 0
                then patients.[id] <- Clinics.Dto.Patient.FromDomain patient; Ok ()
                else Error ["No such patient"]

        let getById id :Result<Clinics.Domain.Patient.T,string list> = 
            if id <= Array.length patients-1 && id >= 0
                then patients.[id] |> Clinics.Dto.Patient.ToDomain
                else Error ["No such patient"]