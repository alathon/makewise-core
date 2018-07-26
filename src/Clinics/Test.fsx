#load "Augments.fs"
#load "Common.fs"
#load "Domain.fs"
#load "DTO.fs"
#r @"../../packages/Microsoft.FSharpLu.Json/lib/netstandard2.0/Microsoft.FSharpLu.Json.dll"

open Clinics
open Clinics.Dto
open System
open Microsoft.FSharpLu.Json

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
    DeviceData = Some deviceData
}

let secondaryRoles = [|
    { Tag = "Transplant"; DeviceData = None; }
|]

let cprNumber = "1212121234"
let patientDto = {
    Name = name
    ContactDetails = contactDetails
    PrimaryRole = primaryRole
    SecondaryRoles = secondaryRoles
    CprNumber = cprNumber
}

let patientDomain = Patient.ToDomain patientDto

// TODO: Show JSON serialized version of patientDto
// Also, for some reason, some of these lines cause VSCode to mess up running the file...
printfn "DTO: %A" patientDto
printfn "Domain: %A" patientDomain