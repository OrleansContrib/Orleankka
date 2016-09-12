module ActorBuilderTests

open ActorExpression

let testActor = actor{
                        actorType "test"
                        body (
                            fun() ->
                                let mutable state = -10000
                                handlers{
                                    onReceive(fun (msg:obj) ->
                                            state <- state + 1
                                            null|> response )
                                    onActivate (fun ()->
                                            state <- 1
                                            taskDone()
                                    )
                                }
                        )
                    }

ActorsRegister.register()
|> Seq.iter  (fun a -> printfn "actorType: %s" a.Id)