GR Botting Library
==================
This projects contains parts of an automated gameplay (ie. botting) framework. It has been used for various problems and games, such as Poker, Backgammon and Blackjack. Parts of the code has been used by the author to create bots for fun for Bejeweled and Tetris Battle as well.

Large part of the code has been in use last time in 2010 and hasn't been maintained or tested after that.

GR.IO
-----
PathHelper generates auto-incrementing unique filename in a given folder.

GR.Net.Mail
-----------
Contains a simple GMail client for sending emails.

GR.Math
-------
Combin and Perm allow enumerating combinations and permutations with integers.

CombinBuilder<T> and PermBuilder<T> allow enumerating combinations and permutations of any type of collection using generics.

Combin, Perm, CombinBuiler and PermBuilder all remember the current state of the enumeration and compute the next combination or permutation based on the current one. PermBuilder uses lexicographic ordering.

Gaussian implements the Box-Muller algorithm for generating normal distribution numbers.

Prime contains very naive and slow method for checking primeness of a number. It also can generate prime numbers.