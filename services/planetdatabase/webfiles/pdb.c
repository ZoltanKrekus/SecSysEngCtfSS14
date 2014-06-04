#include <stdio.h>
#include <string.h>
#include <sys/types.h>
#include <sys/stat.h>
#include <unistd.h>
#include <stdlib.h>
#include <limits.h>

int main(int argc, char **argv) {

    printf("Starting...\nPlanetDatabase Stardate 0.8.15...\n\n");

    if ((argc < 2) || (strncmp(argv[1], "-h", 2) == 0)) {
        printf("\nUsage: pdb [NUMBER_OF_DOCUMENT]\n");
        return(1);
    }

    char filename[PATH_MAX];
    strncpy(filename, "./planets/", 12);
    strncat(filename, argv[1], PATH_MAX - strlen(filename) - 5);

	struct stat res;
	if (stat(filename, &res) != 0) {
		fprintf(stderr, "ERROR: stat() failed for file %s!\n\nQuitting...\n", filename);
		return 1;
	}

	char cmd[PATH_MAX];
	strncpy(cmd, "cat ", 5);
	strncat(cmd, filename, PATH_MAX - strlen(cmd) - 1);

    int err = -1;
    if ((err = system(cmd)) != 0) {
        fprintf(stderr, "ERROR: Data %s doesn't exist!\n\nQuitting...\n", argv[1]);
        return 1;
    }

    return EXIT_SUCCESS;
}
