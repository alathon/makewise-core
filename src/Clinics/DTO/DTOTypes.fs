namespace Clinics.Dto

open System
open Clinics

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
    Email: EmailAddress
}

type PatientRole = {
    Tag: string // e.g., PaceMaker | Transplant
    DeviceData: DeviceData // Data in case of PaceMaker role
}

type Patient = {
    PrimaryRole: PatientRole
    SecondaryRoles: PatientRole[]
    CprNumber: string
    Name: Name
    ContactDetails: ContactDetails
    CreatedAt: DateTime
}
