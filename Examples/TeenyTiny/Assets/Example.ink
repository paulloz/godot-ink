EXTERNAL Animate(id)

LIST Inventory = ChestKey, Mushrooms

-> DONE

=== Sign
    Ah yes...
    That's indeed my house. -> DONE

=== Chest
    {
        - Chest.Open:
            Completely empty. -> DONE
        - Chest <= 1:
            Hey, look! A chest! A chest!
            I wonder what's inside.
            Let's open it.
        - Chest > 1 && Inventory !? ChestKey:
            { Chest > 3: How many times do we have to go over this? }
            Yes, I know, a chest.
            But I still don't have the [color=orange]key[/color] for it. -> DONE
        - else:
            Come on, let's finally open this.
    }
    ...
    { Inventory ? ChestKey:
        -> Open
    - else:
        It's locked.
        The [color=orange]key[/color] is probably lying around somewhere. -> DONE
    }
    = Open
        ~ Animate("Chest")
        ~ Inventory += Mushrooms
        There's only a bunch of old mushrooms in there.
        Exciting, I guess.
        Who knows what I could do with that. -> DONE

=== PickChestKeyUp
    { Chest:
        Cool, the key for the [color=orange]chest[/color] I saw earlier.
        I'll bag that
    - else:
        I found a key!
        I'm pretty sure there [color=orange]something[/color] it could open somewhere.
    }
    ~ Inventory += ChestKey
     -> DONE

=== FlowerGirl
    [rainbow]Hello![/rainbow]
    + Hum, hello?
    + Who are you?
    - -> DONE
