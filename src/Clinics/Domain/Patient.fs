namespace Clinics.Domain

open System
open WrappedString
open System

type CprNumber = private CprNumber of string
module CprNumber =
    let make s = 
        if String.IsNullOrEmpty(s) || not(System.Text.RegularExpressions.Regex.IsMatch(s,@"^\d{10}$"))
            then Error ["Invalid CPR"]
            else Ok (CprNumber s)
    
    let value (CprNumber cpr) = cpr

type Name = {
    FirstName: String100
    Initial: String1 option
    LastName: String100
}

type Address = {
    Line1: String100
    Line2: String100 option
}

type ContactDetails = {
    Address: Address
    Phone: SafeString
    Email: String100 option
}

type DeviceType =
    | ICD
    | LR

type Device = {
    Type: DeviceType
    ImplantedAt: DateTime
    SerialNumber: String100
    ImplantationReason: SafeString option
    Name: String100
    LastReportedBatteryLifetime: int
}

type Patient = {
    Job: SafeString option
    LocalHospital: SafeString option
    Device: Device option
    CprNumber: CprNumber
    Name: Name
    ContactDetails: ContactDetails
}