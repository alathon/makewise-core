namespace Clinics.Dto

open Clinics
open Clinics.Domain.WrappedString
open Result

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Name = 
    let ToDomain (dto:Name) :Result<Domain.Name,string list> = 
        result {
            let! first = string100 "First" dto.FirstName
            let! last = string100 "Last" dto.LastName
            let initial = string1 "Initial" dto.Initial |> asOption
            return { FirstName = first; Initial = initial; LastName = last}
        }
    let FromDomain (name:Domain.Name) :Name =
        {
            FirstName = name.FirstName |> value
            Initial = name.Initial |> valueOrDefault ""
            LastName = name.LastName |> value
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Address =
    let ToDomain (dto:Address) :Result<Domain.Address,string list> =
        result {
            let! line1 = string100 "Address Line1" dto.Line1
            let line2 = 
                match string100 "Address Line2" dto.Line2 with
                | Ok l -> Some l
                | Error _ -> None
            return { 
                Line1 = line1
                Line2 = line2
            }
        }
    let FromDomain (addr:Domain.Address) :Address =
        {
            Line1 = value addr.Line1
            Line2 = addr.Line2 |> valueOrDefault ""
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Device =
    let ToDomain (dto:Device) :Result<Domain.Device,string list> =
        result {
            let! serialNumber = string100 "Serial Number" dto.SerialNumber
            let! name = string100 "Device Name" dto.Name
            let implantationReason = safeStr "Implantation Reason" dto.ImplantationReason |> asOption
            let! deviceType = 
                match dto.Type with
                | "ICD" -> Ok Domain.DeviceType.ICD
                | "LR" -> Ok Domain.DeviceType.LR
                | _ -> Error ["Invalid DeviceType " + dto.Type]

            return {
                Type = deviceType
                ImplantedAt = dto.ImplantedAt
                SerialNumber = serialNumber
                ImplantationReason = implantationReason
                Name = name
                LastReportedBatteryLifetime = dto.LastReportedBatteryLifetime
            }
        }

    let FromDomain (device:Domain.Device) =
        {
            Type = device.Type.ToString()
            ImplantedAt = device.ImplantedAt
            SerialNumber = value device.SerialNumber
            ImplantationReason = device.ImplantationReason |> valueOrDefault ""
            Name = value device.Name 
            LastReportedBatteryLifetime = device.LastReportedBatteryLifetime
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module ContactDetails =
    let FromDomain (contactDetails:Domain.ContactDetails) :ContactDetails =
        let addr = {
            Line1 = value contactDetails.Address.Line1
            Line2 = contactDetails.Address.Line2 |> valueOrDefault ""
        }

        {
            Address = addr
            Phone = value contactDetails.Phone
            Email = contactDetails.Email |> valueOrDefault ""
        }


    let ToDomain (dto:ContactDetails) :Result<Domain.ContactDetails,string list> =
        result {
            let! line1 = string100 "Address Line1" dto.Address.Line1
            let line2 = string100 "Address Line2" dto.Address.Line2 |> asOption

            let addr :Domain.Address = {
                Line1 = line1
                Line2 = line2
            }
            
            let! phone = safeStr "PhoneNumber" dto.Phone
            let email = string100 "Email" dto.Email |> asOption
            
            return {
                Address = addr
                Phone = phone
                Email = email    
            }
        }

[<CompilationRepresentationAttribute(CompilationRepresentationFlags.ModuleSuffix)>]
module Patient =
    let FromDomain (patient:Domain.Patient) :Patient =
        let name = Name.FromDomain patient.Name
        let contactDetails = ContactDetails.FromDomain patient.ContactDetails

        let device = 
            match patient.Device with
            | Some d -> Device.FromDomain d
            | None -> Unchecked.defaultof<Device>

        {
            Job = patient.Job |> valueOrDefault ""
            LocalHospital = patient.LocalHospital |> valueOrDefault ""
            Name = name
            ContactDetails = contactDetails
            CprNumber = patient.CprNumber |> Domain.CprNumber.value
            Device = device
        }

    let ToDomain (dto:Patient) :Result<Domain.Patient,string list> = result {
        let! name = 
            match box dto.Name with
            | null -> Error ["Invalid name"]
            | _ -> Name.ToDomain dto.Name

        let! contactDetails =
            match box dto.ContactDetails with
            | null -> Error ["Invalid contactDetails"]
            | _ -> ContactDetails.ToDomain dto.ContactDetails
        
        let device =
            match box dto.Device with
            | null -> None
            | _ -> Device.ToDomain dto.Device |> asOption

        let! cprNumber = Domain.CprNumber.make dto.CprNumber
        let job = safeStr "Job" dto.Job |> asOption
        let localHospital = safeStr "LocalHospital" dto.LocalHospital |> asOption

        return {
            Job = job
            LocalHospital = localHospital
            Name = name
            ContactDetails = contactDetails
            CprNumber = cprNumber
            Device = device
        }
    }