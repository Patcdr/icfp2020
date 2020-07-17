N		Number
xN	Variable
t		True
f		False

ap f x	Apply f(x)
	Currying enabled
	The number of "ap" on the line indicate the number of args

ap inc N -> N`		Increment
ap dec N -> N`		Decrement
ap neg N -> -N  	Negate
ap pwr2 N -> 2^N 	Power2
ap i N -> N				Identity
ap nil X -> t			Nil
ap isnil X -> t/f Is Nil

ap ap add N1 N2 -> N`		Addition
ap ap mul N1 N2 -> N`		Multiply
ap ap div N1 N2 -> N`		Divide
ap ap eq N1 N2 -> t/f		Equals
ap ap lt N1 N2 -> t/f   Less Than
ap ap t X Y -> X				Truth
ap ap f X Y -> Y  			Falsity

ap ap ap if0 X Y Z -> Y/Z

COMBINATORS
ap ap ap s X Y Z -> X(Z, Y(Z))
ap ap ap c X Y Z -> X(Z, Y)
ap ap ap b X Y Z -> X(Y(Z))

CONS CELLS
ap ap cons X Y -> <X,Y>
ap (ap ap cons X Y) Z -> Z(<X,Y>)
vec = cons	Alias
ap car <X,Y> -> X
ap cdr <X,Y> -> Y

LISTS
( ) -> nil
( X ) = <X,nil> = [X]
( X, Y ) = <X,<Y,nil>> = [X, Y]

I/O
ap mod N -> N`				Change Number Encoding (Grid to Linear)
ap dem N -> N`				Change Number Encoding (Linear to Grid)
ap mod LIST -> LIST`	Modulate all in list
ap send N -> M  			Send message to Aliens, receive response
ap send LIST -> LIST2 Send list message to Aliens, receive list response

ap draw LIST -> PICTURE
ap ap checkerboard N 0 -> PICTURE
ap multipledraw <X,Y> -> PICTURE for each element of CONS List
