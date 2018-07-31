namespace Web

open Clinics.Dto
open System

module private Fake =
    let makePatient :Patient =
        let name = { FirstName = "Joe"; Initial = "G"; LastName = "Schmoe" }
        let address = { 
            Line1 = "Frederikssundsvej 94F, 1.TH"
            Line2 = null 
        }
        let email = "martin@itsolveonline.net"
        let phone = "+45 31424342"
        let contactDetails = { Address = address; Phone = phone; Email = email }
        let device = {
            Type = "ICD"
            ImplantedAt = DateTime.UtcNow
            SerialNumber = "1234567"
            ImplantationReason = "For giving of life"
            Name = "AwesomeDevice(tm) CRX-20000"
            LastReportedBatteryLifetime = 25
        }

        let cprNumber = "1212121234"
        let job = "Brogrammer"
        let localHospital = "Herlev"

        {
            Name = name
            ContactDetails = contactDetails
            CprNumber = cprNumber
            Device = device
            Job = job
            LocalHospital = localHospital
        }

// A fake in-memory DB, which just stores the Patient.Dto's.
// Normally you'd be storing the SQLProvider's generated type,
// and so we would normally need an explicit toDomain function
// from the SQLProvider's generated type to the domain type.
// For now we use the Dto's existing ToDomain function for brevity.
module Db =
    module Patients =
        let mutable private patients :Patient[] = seq { for _ in 0 .. 9 do yield Fake.makePatient } |> Seq.toArray

        let setById id patient =
            if id <= Array.length patients-1 && id >= 0
                then patients.[id] <- Clinics.Dto.Patient.FromDomain patient; Ok ()
                else Error ["No such patient"]

        let getById id = 
            if id <= Array.length patients-1 && id >= 0
                then patients.[id] |> Clinics.Dto.Patient.ToDomain
                else Error ["No such patient: "]