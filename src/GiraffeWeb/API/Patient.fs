namespace Web

open Giraffe
open Clinics.Dto
open Microsoft.AspNetCore.Http
open Clinics

module Patient =
    module private Impl =
        open Result
        let getById i :HttpHandler = 
            match Db.Patients.getById i with
                | Ok p -> p |> json
                | Error e -> e |> List.toSeq |> String.concat "," |> text

        let setById id : HttpHandler =
            fun (next: HttpFunc) (ctx: HttpContext) ->
                task {
                    // HTTP req body -> DTO
                    let! patient = ctx.BindJsonAsync<Clinics.Dto.Patient>()
                    let change = result {
                        // DTO -> Domain -> DTO -> DB action
                        let recipe = 
                            Patient.ToDomain 
                            >=> switch Patient.FromDomain 
                            >=> Db.Patients.setById id

                        return! recipe patient
                    }

                    return! match change with
                            | Ok p -> Successful.OK p next ctx
                            | Error e -> Successful.OK e next ctx
                }
    let api :HttpHandler = 
        choose [
            GET >=> routef "/get/%i" Impl.getById
            POST >=> routef "/set/%i" Impl.setById
            GET >=> text "Hello world!"
        ]