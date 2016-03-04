
SOURCE_DIR = ./source/*.cs

all:
	mcs -debug -target:library -out:ai.dll -reference:./lib/so2ai.dll $(SOURCE_DIR)

test:
	# run this ai on the so2tool