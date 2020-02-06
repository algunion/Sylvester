﻿namespace Sylvester.Arithmetic

open System

type Number = 
  abstract IntVal:int
  abstract Val:int64
  abstract UVal:uint64

type StringHash = Number

[<AutoOpen>]
module Number = 
    let number<'n when 'n :> Number> = Activator.CreateInstance<'n>()
    