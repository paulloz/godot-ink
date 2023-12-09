EXTERNAL Animate(id)
EXTERNAL ShakeWindow()

INCLUDE ExampleHouse.ink

LIST Inventory = ChestKey, Mushrooms, Flowers

aaa -> FlowerGirl

=== Sign
    Ah yes...
    That's indeed my house.
    -> DONE

=== Chest
    {
        - Chest.Open:
            Completely empty.
            -> DONE
        - Chest <= 1:
            Hey, look! A chest! A chest!
            I wonder what's inside.
            Let's open it.
        - Chest > 1 && Inventory !? ChestKey:
            { Chest > 3: How many times do we have to go over this? }
            Yes, I know, a chest.
            But I still don't have the [color=orange]key[/color] for it.
            -> DONE
        - else:
            Come on, let's finally open this.
    }
    ...
    { Inventory ? ChestKey:
        -> Open
    - else:
        It's locked.
        The [color=orange]key[/color] is probably lying around somewhere.
        -> DONE
    }
    = Open
        ~ Animate("Chest")
        ~ Inventory += Mushrooms
        There's only a bunch of old [color=orange]mushrooms[/color] in there.
        Exciting, I guess.
        Who knows what I could do with that.
        -> DONE

=== FlowerGirl
    Hello there!
    { Traded: -> DONE }
    * [Who are you?]
        I've been your neighbour for twenty-six years.
        You think you'd remember my face by now.
        ...
        ~ ShakeWindow()
        [shake]Smack![shake]
    + [Hum, hello.]
        Hi neighbour. How are you?
        ++ [I'm fine.]
            Glad to hear that!
            Personally, <>
        ++ [Why do you care?]
            Just, you know, being a decent human being.
            Maybe you should try someday.
            Anyway. <>
        -- I've been running around all morning.
        I'm making a stew, but it lacks a little something...
        Maybe I could add [color=orange]something tasty[/color]. But what?
    + [Can I have flowers?]
        { Trade <= 0: Depends... | I already told you. }
        -> Trade
    - -> DONE
= Trade
    Do you have something to trade for them?
    { Inventory :
        + { Inventory ? Mushrooms } [Give the mushrooms.]
            -> Traded
        + { Inventory ? ChestKey } [Give the key.]
            What am I supposed to do with this?
            -> DONE
        + [No.]
            Well, I'll be there when you do.
            -> DONE
    - else:
        Because I'm not giving you my flowers for nothing!
        -> DONE
    }
= Traded
    Mushrooms!
    Of course!
    My stew's gonna be perfect now!
    ~ Inventory -= Mushrooms
    ~ Inventory += Flowers
    Thank you.
    -> DONE
