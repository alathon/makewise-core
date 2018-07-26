namespace Clinics

[<AutoOpen>]
module SharedTypes =
    type DeviceData = {
        FirstImplantation: System.DateTime;
        DateOfLastShock: System.DateTime option;
        ActivatedAt: System.DateTime;
    }