﻿namespace Sylvester

open System.Collections

open Sylvester.Arithmetic
open Sylvester.Collections

/// Set of elements closed under a left-associative commutative operation and a 2nd left-associative distributive operation.
type IRing<'t when 't: equality> =  
    inherit IGroup<'t>
    abstract member Op2:BinaryOp<'t>

/// Set of elements closed under a left-associative commutative operations and a 2nd left-associative distributive operation.
type Ring<'t when 't: equality>(group: AbelianGroup<'t>, monoid: Monoid<'t>) =
    inherit Struct<'t, card.two>(group.Set, arrayOf2 (group.Ops.[N10.zero]) (monoid.Ops.[N10.zero]))
    do monoid.Op |> failIfNotDistributiveOver group.Op
    member val Op = group.Op
    member val Op2 = monoid.Op
    member val Group = group
    member val Monoid = monoid
    interface IRing<'t> with
        member val Set = group.Set
        member val Op = group.Op
        member x.GetEnumerator(): Generic.IEnumerator<'t * 't * 't> = (let s = x.Set :> Generic.IEnumerable<'t> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, (group.Op) a b))).GetEnumerator()
        member x.GetEnumerator(): IEnumerator = (x :> Generic.IEnumerable<'t * 't * 't>).GetEnumerator () :> IEnumerator
        member val Identity = group.Identity
        member val Inverse = group.Inverse
        member val Op2 = monoid.Op

    new (set:ISet<'t>, op: BinaryOp<'t>, op2: BinaryOp<'t>, zero:'t, one:'t, inv:UnaryOp<'t>) =
        Ring(AbelianGroup(set, op, zero, inv), Monoid(set, op2, one))

/// Ring with a commutative monoid.
type CommutativeRing<'t when 't: equality>(group: AbelianGroup<'t>, Monoid: CommutativeMonoid<'t>) =
    inherit Ring<'t>(group, Monoid)
    new (set:ISet<'t>, op: BinaryOp<'t>, op2: BinaryOp<'t>, zero:'t, one:'t, inv:UnaryOp<'t>) =
        CommutativeRing(AbelianGroup(set, op, zero, inv), CommutativeMonoid(set, op2, one))

/// Commutative ring with a total order relation.
type OrderedRing<'t when 't: equality and 't : comparison>(group: AbelianGroup<'t>, monoid: CommutativeMonoid<'t>, order: Order<'t>) =
    inherit CommutativeRing<'t>(group, monoid)
    interface ITotalOrder<'t> with
        member val Set = group.Set
        member val Order = order
    interface Generic.IEnumerable<'t * 't * bool> with
        member x.GetEnumerator(): Generic.IEnumerator<'t * 't * bool> = 
            (let s = x.Set :> Generic.IEnumerable<'t> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, (order) a b))).GetEnumerator()
        member x.GetEnumerator(): IEnumerator = (let s = x.Set :> Generic.IEnumerable<'t> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, (order) a b))).GetEnumerator() :> IEnumerator

[<AutoOpen>]
module Ring =
    let Zero = CommutativeRing(Group.Zero, Monoid.Zero)
    
    let inline AdditiveRing<'t when 't : equality and 't : (static member Zero:'t) and 't: (static member (+) :'t -> 't -> 't) and 't: (static member (~-) :'t -> 't)>(set:ISet<'t>, op, id) =
        Ring(AdditiveGroup(set), Monoid(set, op, id))

    let Zpos =
        let set = infiniteSeq  (fun x -> x >= 0) (id)
        let order = (<=)
        {
            new OrderedRing<int>(AdditiveGroup(set), MultiplicativeMonoid(set), order) 
                interface IWellOrder<int> with
                    member x.Least(e:Set<int>) = LanguagePrimitives.GenericZero
                interface ITotalOrder<int> with
                    member x.Set = set
                    member x.Order = order
                    member x.GetEnumerator(): Generic.IEnumerator<int * int * bool> = 
                        (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, order a b))).GetEnumerator()
                    member x.GetEnumerator(): IEnumerator = (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, ((<)) a b))).GetEnumerator() :> IEnumerator
        }

    let Zneg =
        let set = infiniteSeq  (fun x -> x >= 0) (fun n -> -n)
        let order = (<=)
        {
            new OrderedRing<int>(AdditiveGroup(set), MultiplicativeMonoid(set), order) 
                interface IWellOrder<int> with
                    member x.Least(e:Set<int>) = LanguagePrimitives.GenericZero
                interface ITotalOrder<int> with
                    member x.Set = set
                    member x.Order = order
                    member x.GetEnumerator(): Generic.IEnumerator<int * int * bool> = 
                        (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, order a b))).GetEnumerator()
                    member x.GetEnumerator(): IEnumerator = (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, ((<)) a b))).GetEnumerator() :> IEnumerator
        }

    let Z =
        let set = Zpos |+| Zneg
        let order = (<=)
        {
            new OrderedRing<int>(AdditiveGroup(set), MultiplicativeMonoid(set), order) 
                interface IWellOrder<int> with
                    member x.Least(e:Set<int>) = LanguagePrimitives.GenericZero
                interface ITotalOrder<int> with
                    member x.Set = set
                    member x.Order = order
                    member x.GetEnumerator(): Generic.IEnumerator<int * int * bool> = 
                        (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, order a b))).GetEnumerator()
                    member x.GetEnumerator(): IEnumerator = (let s = x.Set :> Generic.IEnumerable<int> in s |> Seq.pairwise |> Seq.map (fun(a, b) -> (a, b, ((<)) a b))).GetEnumerator() :> IEnumerator
        }

    let Z1 = CommutativeRing(Z, Mod.(+) 1, Mod.(*) 1, 0, 1, (~-))
    let Z2 = CommutativeRing(Z, Mod.(+) 2, Mod.(*) 2, 0, 1, (~-))
    let Z3 = CommutativeRing(Z, Mod.(+) 3, Mod.(*) 3, 0, 1, (~-))
    let Z4 = CommutativeRing(Z, Mod.(+) 4, Mod.(*) 4, 0, 1, (~-))
    let Z5 = CommutativeRing(Z, Mod.(+) 5, Mod.(*) 5, 0, 1, (~-))
    let Z6 = CommutativeRing(Z, Mod.(+) 6, Mod.(*) 6, 0, 1, (~-))
    let Z7 = CommutativeRing(Z, Mod.(+) 7, Mod.(*) 7, 0, 1, (~-))
    let Z8 = CommutativeRing(Z, Mod.(+) 8, Mod.(*) 8, 0, 1, (~-))
    let Z9 = CommutativeRing(Z, Mod.(+) 9, Mod.(*) 9, 0, 1, (~-))