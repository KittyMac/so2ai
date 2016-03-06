
SOURCE_DIR = ./source/*.cs

all:
	mcs -sdk:2 -debug -target:library -out:ai.dll -r:./lib/so2ai.dll $(SOURCE_DIR)

test:
	# run this ai on the so2tool