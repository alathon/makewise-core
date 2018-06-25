#load "Clinics.fs"

open Clinics;
open System;

module Tst =
    let deviceData = { FirstImplantation = DateTime.Now; ActivatedAt = DateTime.Now; DateOfLastShock = None }

    let myPatient = {
        PrimaryRole = PaceMakerRole deviceData;
        SecondaryRoles = [];
        CprNumber = CprNumber.make "1212121111" |> Option.get;
        Name = Name.Create "Joe" "G" "Schmoe" |> Option.get
        ContactDetails = {
            Address = { line1 = "Frederikssundsvej 94F, 1.TH"; line2 = Some "2400 Copenhagen NV"; };
            Phone = Some "+45 12345678";
            Email = EmailAddress.create "martin@makewise.io";
        }
    }

printfn "%A" Tst.myPatient