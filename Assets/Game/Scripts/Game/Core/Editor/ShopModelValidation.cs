using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace RosiesCornerShop.Core.Editor
{
    public static class ShopModelValidation
    {
        [MenuItem("Tools/Rosie's Corner Shop/Validate game model")]
        public static void Validate()
        {
            int moves = 0, completed = 0;
            for (int level = 0; level < 20; level++)
            {
                var progress = new GameProgress { coins = 100000, sound = false };
                var session = new GameSession(progress,null); session.StartLevel(level); float clock = 0;
                for (int step = 0; step < 700; step++)
                {
                    Check(session.State); session.Tick(clock += 2); Check(session.State);
                    if (session.Won) { completed++; break; }
                    GameState s = session.State;
                    if (session.Lost)
                    {
                        if (level >= 5 && session.Boost(0)) continue;
                        if (level >= 3 && s.counter.Count > 0 && session.Boost(1)) continue;
                        break; // Loss is a valid terminal state, not a solver assertion.
                    }
                    var choices = s.columns.SelectMany(c => c.shelves).Where(b => b.Open)
                        .SelectMany(b => b.slots.Select((j,i) => new { b,j,i })).Where(p => p.j != null).ToList();
                    var choice = choices.FirstOrDefault(p => p.j.hammer || s.baskets.Any(b => b != null && b.count < 3 && b.type == p.j.type));
                    if (choice == null && s.counter.Count < s.capacity) choice = choices.FirstOrDefault();
                    if (choice == null) continue;
                    int before = Remaining(s); int beforeMoves = s.moves;
                    if (!session.Take(choice.b,choice.i)) throw new Exception("Legal move rejected");
                    Check(s); moves++;
                    if (step == 0 && level >= 1)
                    {
                        if (!session.Undo() || Remaining(session.State) != before || session.State.moves != beforeMoves)
                            throw new Exception("Undo did not restore the model");
                        Check(session.State);
                    }
                    if (level >= 3 && step % 13 == 7 && session.State.counter.Count > 0) session.Boost(1);
                    if (step % 17 == 8) session.Boost(2);
                    Check(session.State);
                }
            }
            CheckRules();
            string result = "PASS: 20 level configurations; " + moves + " legal moves; " + completed
                + " greedy playthroughs completed. Conservation, unique item IDs, capacities, undo, key, hammer, ice, bags and rear rows validated.";
            Directory.CreateDirectory("Temp"); File.WriteAllText("Temp/RosieValidation.txt",result); Debug.Log(result);
        }
        static GameSession Fixture()
        {
            var session = new GameSession(new GameProgress { sound = false },null); session.StartLevel(1);
            var s = session.State; s.columns.Clear(); s.columns.Add(new ColumnState()); s.orders.Clear(); s.counter.Clear();
            s.baskets = new BasketState[3]; s.jps = 2; s.visible = 3; s.freezes = 0; s.total = 0; s.nextId = 1000;
            return session;
        }
        static ShelfState Shelf(GameSession session, params ItemState[] items)
        {
            var shelf = new ShelfState { id = session.State.nextId++, height = 1, slots = items.ToList() };
            session.State.columns[0].shelves.Add(shelf); session.State.total += items.Sum(Count); return shelf;
        }
        static void CheckRules()
        {
            var session = Fixture(); var target = Shelf(session,new ItemState { id = 1,type = 1 },null); target.locked = true;
            var key = Shelf(session,new ItemState { id = 2,type = 0 },null); key.key = target.id;
            if (session.Take(target,0)) throw new Exception("Locked shelf accepted a move");
            session.Take(key,0); if (target.locked) throw new Exception("Key did not unlock shelf"); Check(session.State);
            if (!session.Undo() || !session.State.columns[0].shelves[0].locked) throw new Exception("Undo did not restore lock");
            session = Fixture(); target = Shelf(session,new ItemState { id = 3,type = 0 },null); target.boarded = true;
            var tool = Shelf(session,new ItemState { id = 4,hammer = true,target = target.id },null);
            session.Take(tool,0); if (target.boarded) throw new Exception("Hammer failed"); Check(session.State);
            session = Fixture(); var rear = new ItemState { id = 6,type = 1 };
            var bag = Shelf(session,new ItemState { id = 5,type = 0,back = rear },new ItemState { id = 7,type = 2,wrapped = true });
            session.Take(bag,0); if (bag.slots[0] != rear || bag.slots[1].wrapped) throw new Exception("Rear row or unwrap failed"); Check(session.State);
            session = Fixture(); target = Shelf(session,new ItemState { id = 8,type = 1 },null); target.frozen = 1;
            var source = Shelf(session,new ItemState { id = 9,type = 0 },null);
            session.State.total += 2;
            session.State.baskets[0] = new BasketState { id = 30,type = 0,items = new List<ItemState> { new ItemState { id = 10,type = 0 },new ItemState { id = 11,type = 0 } } };
            session.Take(source,0); session.Tick(2); if (target.frozen != 0 || session.State.collected != 3) throw new Exception("Shipping or ice failed"); Check(session.State);
            session = Fixture(); source = Shelf(session,new ItemState { id = 12,type = 0 },null);
            for (int i = 0; i < 5; i++) { session.State.counter.Add(new ItemState { id = 20 + i,type = 1 }); session.State.total++; }
            int before = Remaining(session.State); if (session.Take(source,0) || Remaining(session.State) != before) throw new Exception("Full counter accepted an unmatched item");
            Check(session.State);
        }
        static int Count(ItemState item) => item == null ? 0 : (item.hammer ? 0 : 1) + Count(item.back);
        static IEnumerable<ItemState> Flatten(ItemState item)
        { while (item != null) { yield return item; item = item.back; } }
        static IEnumerable<ItemState> Items(GameState s)
        {
            return s.counter.Concat(s.columns.SelectMany(c => c.queue))
                .Concat(s.columns.SelectMany(c => c.shelves).SelectMany(b => b.slots))
                .Concat(s.baskets.Where(b => b != null).SelectMany(b => b.items)).SelectMany(Flatten);
        }
        static int Remaining(GameState s) => Items(s).Count(i => !i.hammer);
        static void Check(GameState s)
        {
            if (Remaining(s) + s.collected != s.total) throw new Exception("Item conservation failed on level " + (s.level + 1));
            var items = Items(s).ToArray(); if (items.Select(i => i.id).Distinct().Count() != items.Length) throw new Exception("Duplicate item ID");
            if (s.counter.Count > s.capacity || s.baskets.Any(b => b != null && b.count > 3)) throw new Exception("Capacity overflow");
            foreach (ColumnState c in s.columns)
            {
                if (c.shelves.Sum(b => b.height) > s.visible) throw new Exception("Column overflow");
                if (c.shelves.Any(b => b.slots.Count != b.height * s.jps || b.slots.All(j => j == null))) throw new Exception("Invalid shelf");
            }
        }
    }
}
