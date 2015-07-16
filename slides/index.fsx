(**
- title : Domain Modeling with Types
- description : Domain Modeling with Types (using F#)
- author : Ryan Riley (@panesofglass)
- theme : night
- transition : default

***

*)

(*** hide ***)
#r "System.Device.dll"
#I "../packages"
#r "FSharp.Data.SqlClient/lib/net40/FSharp.Data.SqlClient.dll"
#r "Aether/lib/net40/Aether.dll"
#r "Hekate/lib/net40/Hekate.dll"
#r "FParsec/lib/net40-client/FParsecCS.dll"
#r "FParsec/lib/net40-client/FParsec.dll"
#r "Chiron/lib/net45/Chiron.dll"
#r "Arachne.Core/lib/net45/Arachne.Core.dll"
#r "Arachne.Uri/lib/net45/Arachne.Uri.dll"
#r "Arachne.Uri.Template/lib/net45/Arachne.Uri.Template.dll"
#r "Arachne.Language/lib/net45/Arachne.Language.dll"
#r "Arachne.Http/lib/net45/Arachne.Http.dll"
#r "Arachne.Http.Cors/lib/net45/Arachne.Http.Cors.dll"
#r "Freya.Core/lib/net45/Freya.Core.dll"
#r "Freya.Lenses.Http/lib/net45/Freya.Lenses.Http.dll"
#r "Freya.Lenses.Http.Cors/lib/net45/Freya.Lenses.Http.Cors.dll"
#r "Freya.Recorder/lib/net45/Freya.Recorder.dll"
#r "Freya.Machine/lib/net45/Freya.Machine.dll"
#r "Freya.Machine.Extensions.Http/lib/net45/Freya.Machine.Extensions.Http.dll"
#r "Freya.Machine.Extensions.Http.Cors/lib/net45/Freya.Machine.Extensions.Http.Cors.dll"
#r "Freya.Router/lib/net45/Freya.Router.dll"
#r "Freya.Machine.Router/lib/net45/Freya.Machine.Router.dll"

(**

# Domain Modeling with Types

## [Ryan Riley](https://twitter.com/panesofglass)

***

<img alt="Tachyus logo" src="images/tachyus.png" height="100px" style="background-color:#fff;" />

***

# [@panesofglass](https://twitter.com/panesofglass)

***

## [github/panesofglass](https://github.com/panesofglass)

***

# [OWIN](http://owin.org/)

***

![F# logo](images/fssf.png)

***

# Objective

## Visualize distance between two cities by their geographic coordinates.

***

# Process

*)

(*** include: distance-calculator-example ***)

(**

' This pipeline shows that we want to take a list of two Cities as inputs,
' translate those cities into locations,
' and finally calculate the distance (in feet).

***

# Goal 1

## Define `City`

***

## Simplest thing possible (C#)

    [lang=cs]
    using City = string;

***

## Simplest thing possible (F#)

*)

type CityAlias = string

(**
***

## Does this really help?

' Maybe: you get better documentation throughout your code;
' however, you don't get any type safety.

***

## Can we do better?

***

## Single-cased union types

*)

type City1 = City of string

(** Extract the value via pattern matching: *)
let cityName (City name) = name
let result = cityName (City "Houston, TX")

(** Value of result: *)
(*** include-value: result ***)

(**
***

# Goal 2

## Define a `Location` type

***

## `Place` type (C#)

    [lang=cs]
    public interface ILocatable {
        float Latitude { get; }
        float Longitude { get; }
    }

    public class PlaceFirstTry : ILocatable {
        public string Name { get; set; }
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

***

## `Place` type (F#)

*)

type ILocatable =
    abstract Latitude : float
    abstract Longitude : float

type PlaceFirstTry =
    { Name : string
      Latitude : float
      Longitude : float }
    interface ILocatable with
        member this.Latitude = this.Latitude
        member this.Longitude = this.Longitude

(**
***

## What could go wrong?

1. Latitude or longitude not provided
2. Latitude and longitude mixed up
3. Invalid city and location values

***

## 1. Handling missing values

Examples:

* Atlantis
* Camelot

***

## C# version:

    [lang=cs]
    class Locatable : ILocatable {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class PlaceWithOptionalLocation {
        public string Name { get; set; }
        public float? Latitude { get; set; }
        public float? Longitude { get; set; }
        public ILocatable GetLocation() {
            if (Latitude.HasValue && Longitude.HasValue) {
                return new Locatable {
                    Latitude = Latitude.Value,
                    Longitude = Longitude.Value
                };
            } else {
                return null; // Oh noes!
            }
        }
    }

***

## F# `Place` with optional `ILocatable`

*)

type PlaceWithOptionalLocation =
    { Name : string
      Latitude : float option
      Longitude : float option }
    member this.GetLocation() : ILocatable option =
        match this.Latitude, this.Longitude with
        | Some lat, Some lng ->
            { new ILocatable with
                member this.Latitude = lat
                member this.Longitude = lng } |> Some
        | _ -> None

(**
***

## Yuck, right?

***

## Can we do better?

***

## Rethinking `ILocatable`

### "Is-a" vs. "Has-a"

***

## `Latitude` and `Longitude` belong together

***

## Refactored `Place` type (C#)

    [lang=cs]
    public class Location {
        public float Latitude { get; set; }
        public float Longitude { get; set; }
    }

    public class Place {
        public string Name { get; set; }
        public Location Location { get; set; }
    }

***

## Note: "optional" indicated by a `null`

***

## Refactored `Place` type (F#)

*)

type Location1 = { Latitude : float; Longitude : float }

type Place1 = { Name : string; Location : Location1 option }

(**
***

## Note: No `null`

***

## Has-a preferred to Is-a

### "Composition over inheritance"

***

## 2. Mixing up latitude and longitude

***

## Sadly we will have to leave C# behind

***

## Units of measure

*)

(*** include: location-measure-types ***)

(**
***

## Correct `Location`

*)

(*** include: location2 ***)

(**
***

## Make invalid values impossible

***

## Types can only get you so far

1. `City` allows `null` and `""`
2. `Location` allows latitude < -90 and > 90
3. `Location` allows longitude < -180 and > 180

***

## 3. Valid values

***

## Revisiting `City`

*)

(*** include: city ***)

(**

' Without a city lookup, we are stuck with a minimal
' validation for the city name. We could add a bit
' more, such as requiring one or more commas, but
' this provides the general idea.

***

## Better than nothing

***

## Revisiting `Location`

*)

(*** include: location ***)

(**

' This may look strange as we are now hiding
' the record we previously used. However, we
' retain immutability and add validation.

***

## Revisiting `Place`

*)

(*** include: place ***)

(**
***

## `Place` still has yet to change

### Single Responsibility Principle

' Place doesn't really change except to swap
' Name : string for Name : City
' This is exactly the goal of SRP.

***

## Further study: Contracts and Dependent Types

* [Code Contracts](http://research.microsoft.com/en-us/projects/contracts/)
* [F*](https://fstar-lang.org/)
* [TS*](http://research.microsoft.com/en-us/um/people/nswamy/papers/tstar.pdf)
* [Idris](http://www.idris-lang.org/)

' We have seen that the F# type system can
' get us pretty far along our way. 
' We had to resort to constructor functions
' to provide more rigid validation.

' Note that we can still create an invalid `City`.
' However, if we privatize the union, we can't
' retrieve the name via pattern matching.

' Some languages offer additional type system
' constructs to do these validations. Code contracts,
' originally from Eiffel, offer one form of this.
' Newer languages have also been working on a concpt
' called dependent types or refinement types. These
' allow you to further specify the range of values
' allowed, as we have done above using our
' constructor functions. I've listed a few examples
' in case you wish to study these further.

***

# Goal 3: Processing Requests

***

## Process

*)

(*** include: distance-calculator-example ***)

(**

' This pipeline shows that we want to take a list of two Cities as inputs,
' translate those cities into locations,
' and finally calculate the distance (in feet).

***

## How do we translate?

* UI input: two cities
* Calculation input: two geographic coordinates
* Unit conversion: meters -> feet

***

## Start with types

*)

(*** include: workflow-process-types ***)

(**
***

## Implementation

***

## Retrieving values from other sources

' For the purposes of our demo application,
' let's assume we have a database of city names
' with their corresponding geographic coordinates.
' We will use the FSharp.SqlClient type provider
' to provide us the types we need to access the data.

' Why not use a third-party service such as Google Maps?
' I thought of doing this, but I wanted to keep this
' example close to what we are actually doing at Tachyus.
' Also, the SqlClient type providers are really quite cool,
' and I like showing it off. It's also a way we can
' further constrain the available cities, should we have
' time to explore that.

***

## [FSharp.Data.SqlClient](http://fsprojects.github.io/FSharp.Data.SqlClient/)

***

## Type-checked SQL

*)

(*** include: sql-command-provider ***)

(**

' Show how this works and that invalid SQL
' immediately causes compiler errors.
' Discuss that this erases to raw ADO.NET
' so performance is excellent.

***

## Lookup coordinates by city name

*)

(*** include: lookup-locations ***)

(**

***

## Calculate Distance

*)

(*** include: find-distance ***)

(**

' System.Device.Location.GeoCoordinate has a method
' that uses the Haversine formula, which assumes a
' spherical earth. We could implement this ourselves,
' but why bother when it's already available?

***

## Unit conversion

*)

(*** include: units-of-measure ***)
(*** include: meters-to-feet ***)

(**

' Similar to what we saw above, we apply units of measure
' to ensure our results are what we intended. Note that
' here we create a conversion function to handle the conversion.

***

## Wrapping up the steps

*)

(*** include: distance-calculator-pipeline ***)

(**

' Here we use function composition and rely on type
' inference to create a single function to run our
' workflow.

***

## What about `Show`?

' We introduced a Show type with the other workflow types.
' Where is it? How do we get to the Show step? Doesn't it
' break from the process we defined at the outset?
' How are we going to bridge the gap?

***

# Goal 4: Visualizing Results

***

## What do we need to do?

1. Serialize data for display in a browser
2. Define HTTP resources to process requests
3. Render the UI

***

## 1. Serialization

***

## Serialized data should also send `Place`s

***

## Bridging the gap

*)

(*** include: distance-workflow ***)

(**

' Here we create a mutually recursive function definition,
' with each function calling the next step in succession.
' Note that these essentially wrap the functions we defined
' above and thread through the additional state we want to
' make available at the end. We also introduce the showPlaces
' function that will serialize the results for us.

***

## Defunctionalization

' Not everyone likes the mutually recursive function approach.
' It works, and in some cases it is the best and right solution.
' However, you can defunctionalize this solution to use
' discriminated unions and a single recursive function to process
' the same workflow.

***

## Defining state machine `Stage`s

*)

(*** include: stages ***)

(**
***

## Process `Stage`s recursively

*)

(*** include: processing-stages ***)

(**
***

## Output: JSON strings

***

# Questions?

***

# Resources

* TODO 1
* TODO 2

*)

(*** define: location-measure-types ***)
[<Measure>] type degLat
[<Measure>] type degLng

let degreesLatitude x = x * 1.<degLat>
let degreesLongitude x = x * 1.<degLng>

(*** hide ***)
let degLatResult = degreesLatitude 1.
let degLngResult = degreesLongitude 1.

(*** define: location2 ***)
type Location2 = {
    Latitude : float<degLat>
    Longitude : float<degLng>
}

type Place2 = { Name : City1; Location : Location2 option }

(*** define: city ***)
type City = City of name : string
    with
    static member Create (name : string) =
        match name with
        | null | "" ->
            invalidArg "Invalid city"
                "The city name cannot be null or empty."
        | x -> City x

(*** define: location ***)
type Location =
    private { latitude : float<degLat>; longitude : float<degLng> }
    member this.Latitude = this.latitude
    member this.Longitude = this.longitude
    static member Create (lat, lng) =
        if -90.<degLat> > lat || lat > 90.<degLat> then
            invalidArg "lat"
                "Latitude must be within the range -90 to 90."
        elif -180.<degLng> > lng && lng > 180.<degLng> then
            invalidArg "lng"
                "Longitude must be within the range -180 to 180."
        else { latitude = lat; longitude = lng }

(*** define: place ***)
type Place = { Name : City; Location : Location option }

(*** define: units-of-measure ***)
[<Measure>] type m
[<Measure>] type ft

(*** hide ***)
open FSharp.Data

[<Literal>]
let connStr = """Data Source=(LocalDB)\v11.0;Initial Catalog=Database1;Integrated Security=True;Connect Timeout=10"""

(*** define: sql-command-provider ***)
type GetCityLocation = SqlCommandProvider<"
    SELECT City, Latitude, Longitude
    FROM [dbo].[CityLocations]
    WHERE City = @city
    ", connStr, SingleRow = true>

(*** define: workflow-process-types ***)
type LookupLocations = City * City -> Place * Place

type TryFindDistance = Place * Place -> float<m> option

type MetersToFeet = float<m> -> float<ft>

type Show = Place * Place * float<ft> option -> string

(*** define: lookup-locations ***)
let lookupLocation (City name) =
    use cmd = new GetCityLocation()
    let result = cmd.Execute(name)
    match result with
    | Some p ->
        match p.Latitude, p.Longitude with
        | Some lat, Some lng ->
            { Name = City.Create(p.City)
              Location = Location.Create(
                            degreesLatitude lat,
                            degreesLongitude lng) |> Some }
        | _, _ ->
            { Name = City.Create(p.City); Location = None }
    | None -> { Name = City.Create(name); Location = None }

let lookupLocations (start, dest) =
    lookupLocation start, lookupLocation dest

(*** hide ***)
lookupLocation (City "Houston, TX")
lookupLocation (City "Conroe, TX")
lookupLocation (City "The Woodlands, TX")
lookupLocation (City "Adelaide, AUS")
lookupLocation (City "Atlantis, TX")

(*** define: find-distance ***)
open System.Device.Location
let toGeoCoordinate = function
    { latitude = lat; longitude = lng } ->
        GeoCoordinate(lat / 1.<degLat>, lng / 1.<degLng>)

let findDistance (start: Location, dest: Location) : float<m> =
    (start |> toGeoCoordinate).GetDistanceTo(dest |> toGeoCoordinate)
    * 1.<m>

let tryFindDistance : TryFindDistance = function
    | { Location = Some start }, { Location = Some dest } ->
        findDistance (start, dest) |> Some
    | _, _ -> None

(*** define: meters-to-feet ***)
let metersToFeet (input: float<m>) =
    input * 3.2808399<ft/m>
    
(*** define: distance-calculator-pipeline ***)
let workflow =
    lookupLocations
    >> tryFindDistance
    >> Option.map metersToFeet

(*** define: distance-calculator-example ***)
(City "Houston, TX", City "San Mateo, CA")
|> lookupLocations
|> tryFindDistance
|> Option.map metersToFeet

(*** define: serialize ***)
let serializePlace = function
    | { Place.Name = City name; Location = Some loc } ->
        sprintf """{"name":"%s","latitude":%f,"longitude":%f}""" name loc.Latitude loc.Longitude
    | _ -> "null"

let serializeResult place1 place2 distance =
    let place1' = serializePlace place1
    let place2' = serializePlace place2
    match distance with
    | Some d ->
        sprintf """{"start":%s,"dest":%s,"distance":%f}""" place1' place2' d
    | None -> sprintf """{"start":%s,"dest":%s}""" place1' place2'

(*** define: distance-workflow ***)
let rec receiveInput(start, dest) =
    let start', dest' = lookupLocations(start, dest)
    calculateDistance(start', dest')
and calculateDistance(start, dest) =
    match tryFindDistance(start, dest) with
    | Some distance ->
        showPlacesWithDistanceInMeters(start, dest, distance)
    | None -> showPlaces(start, dest, None)
and showPlacesWithDistanceInMeters(start, dest, distance) =
    showPlaces(start, dest, Some(metersToFeet distance))
and showPlaces(start, dest, distance) =
    serializeResult start dest distance

(*** define: stages ***)
type Stage =
    | AwaitingInput
    | InputReceived of start : City * dest : City
    | Located of start : Place * dest : Place
    | Calculated of Place * Place * float<m> option
    | Show of Place * Place * float<ft> option

(*** define: processing-stages ***)
type RunWorkflow = Stage -> string

let rec runWorkflow = function
    | AwaitingInput -> runWorkflow AwaitingInput
    | InputReceived(start, dest) ->
        runWorkflow (Located(lookupLocations(start, dest)))
    | Located(start, dest) ->
        runWorkflow (Calculated(start, dest, tryFindDistance(start, dest)))
    | Calculated(start, dest, distance) ->
        runWorkflow (Show(start, dest, distance |> Option.map metersToFeet))
    | Show(start, dest, distance) -> serializeResult start dest distance

(*** hide ***)
open System
open System.Text
open Arachne.Language
open Arachne.Uri
open Arachne.Http
open Arachne.Uri.Template
open Chiron
open Freya.Core
open Freya.Core.Operators
open Freya.Lenses.Http
open Freya.Machine
open Freya.Machine.Extensions.Http
open Freya.Machine.Router
open Freya.Router

let inline represent (x : string) =
    { Description =
        { Charset = Some Charset.Utf8
          Encodings = None
          MediaType = Some MediaType.Json
          Languages = Some [ LanguageTag.Parse "en" ] }
      Data = Encoding.UTF8.GetBytes x }

(*** define: parse-query-string ***)
let cities = freya {
    let! qs = Freya.getLens Request.query
    let parts =
        qs.Split('&')
        |> Array.map (fun q ->
            let arr = q.Split('=')
            arr.[1])
    return City.Create(parts.[0]), City.Create(parts.[1]) }

(*** define: get-handler ***)
let get = freya {
    let! cities = cities
    let stage = InputReceived cities
    return runWorkflow stage } |> Freya.memo

let getHandler _ = freya {
    let! json = get
    return represent json }

(*** define: http-config ***)
let en = Freya.init [ LanguageTag.Parse "en" ]
let json = Freya.init [ MediaType.Json ]
let utf8 = Freya.init [ Charset.Utf8 ]

let common =
    freyaMachine {
        using http
        charsetsSupported utf8
        languagesSupported en
        mediaTypesSupported json }

(*** define: resource ***)
let supportedMethods =
    Freya.init [GET; OPTIONS]

let distanceCalculator =
    freyaMachine {
        including common
        methodsSupported supportedMethods
        handleOk getHandler } |> FreyaMachine.toPipeline

let app =
    freyaRouter {
        resource (UriTemplate.Parse "/") distanceCalculator
    } |> FreyaRouter.toPipeline
