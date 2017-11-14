The purpose of this tutorial is to give an introduction to using Orleankka by creating a simple bank account actor using F#.

## Set up your project

Start Visual Studio and create a new F# Console Application. Once we have our console application, we need to open up the Package Manager Console and type:

	PM> Install-Package Orleankka.Fsharp

Then we need to open the relevant namespaces/modules:

```fsharp
open System
open System.Reflection
 
open Orleankka             // base types of Orleankka
open Orleankka.FSharp      // additional API layer for F#
open Orleankka.Playground  // default host configuration
```

First, we need to define a message type that our actor will respond to:

```fsharp
// create an (immutable) message type that actor will respond to
type Message = 
   | Balance
   | Deposit of amount:int
   | Withdraw of amount:int
```

Once we have the message type, we can define our Actor:

```fsharp
// create the actor class
type BankAccount() = 
   inherit Actor<Message>() // tell the actor to respond on Message
  
   // represent a private local state of actor
   let mutable balance = 0

   // this method will be called when message arrived
   override this.Receive message reply = task {
      match message with
      | Balance          -> reply balance 
      | Deposit amount   -> balance <- balance + amount
      | Withdraw amount  ->
         if balance < amount then failwith "amount may not be larger than account balance" 
         else balance <- balance - amount 
   }
```
So what do we have here? We have a **Message** type that represents the **Withdraw**, **Balance**, **Deposit** messages. The **BankAccount** actor, is then told to handle the **Withdraw** message of the type **Message** by subtracting the amount from the balance. If the amount is too large, the actor will throws exception to it’s sender telling it that the operation failed due to a too large amount trying to be withdrawn.

Let's consume our actor, we do so by getting a proxy reference ActorSystem and calling ActorOf:

```fsharp
open System
open System.Reflection
 
open Orleankka             // base types of Orleankka
open Orleankka.FSharp      // additional API layer for F#
open Orleankka.Playground  // default host configuration

// create an (immutable) message type that actor will respond to
type Message = 
   | Balance
   | Deposit of amount:int
   | Withdraw of amount:int

// create the actor class
type BankAccount() = 
   inherit Actor<Message>() // tell the actor to respond on Message
  
   // represent a private local state of actor
   let mutable balance = 0

   // this method will be called when message arrived
   override this.Receive message reply = task {
      match message with
      | Balance          -> reply balance
      | Deposit amount   -> balance <- balance + amount
      | Withdraw amount  ->
         if balance < amount then failwith "amount may not be larger than account balance" 
         else balance <- balance - amount 
   }

[<EntryPoint>]
let main argv = 

    printfn "Running demo. Booting cluster might take some time ...\n"

    // create a new actor system (a container for your actors)
    use system = ActorSystem.Configure()
                            .Playground()
                            .Register(Assembly.GetExecutingAssembly())
                            .Done()

    // create the BankAccount actor and get a reference to it.
    // this will be an "ActorRef", which is not a 
    // reference to the actual actor instance
    // but rather a client or proxy to it          
    let account = system.ActorOf<BankAccount>("Antya")

    task {
      // use (<?) ask operator when you care about result... return type is Task<'Result>
      let! balance = account <? Balance
      printfn "Account balance is %i \n" balance

      printfn "Let's put 100$ on the account \n"
      // use (<!) tell operator when you don't care about result... return type is Task<unit>
      do! account <! Deposit(100)      
  
      printfn "Let's withdraw 50$ \n"
      do! account <! Withdraw(50)

      let! balance = account <? Balance
      printfn "And account balance is %i \n" balance
    } 
    |> Task.wait
    
    Console.ReadLine() |> ignore
    0
```
That is it, your actor is now ready to consume messages sent from any number of calling threads. We now have a completely lock free implementation of an bank account.
