"******************** Counting LOC *******************"
"LOC = " + (gci -include *.java -recurse | select-string .).Count
pause