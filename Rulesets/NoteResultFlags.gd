class_name NoteResultFlags

## Flags for NoteResult. Will prevent the action from being activated.

const NONE : int = 0b00000000 ## Will let every action be triggered.
const HEALTH : int = 0b00000001 ## Will prevent health from being updated if raised.
const SCORE : int = 0b00000010 ## Will prevent the score from being updated if raised.
const SPLASH : int = 0b00000100 ## Will prevent the splash animation from being played if raised.
const ANIMATION : int = 0b00001000 ## Will prevent the sing animation from being played if raised.
