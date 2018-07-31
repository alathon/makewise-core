namespace Clinics.Dto

open System

type Name = {
    FirstName: string
    Initial: string
    LastName: string
}

type Address = {
    Line1: string
    Line2: string
}

type EmailAddress = {
    EmailAddress: string
    IsVerified: bool
}

type ContactDetails = {
    Address: Address
    Phone: string
    Email: string
}

type Device = {
    Type: string
    ImplantedAt: DateTime
    SerialNumber: string
    ImplantationReason: string
    Name: string
    LastReportedBatteryLifetime: int
}

type Patient = {
    Job: string
    LocalHospital: string
    CprNumber: string
    Name: Name
    ContactDetails: ContactDetails
    Device: Device
}