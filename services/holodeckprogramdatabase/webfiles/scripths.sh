#!/bin/bash
CMD=$1

#create db file if not exists
#touch ../db/hsdata
case $CMD in
    store)
        TITEL=$2
        BEMERKUNG=$3
        SECRET=$4
        ./genkey secAdd "$TITEL" "$BEMERKUNG" "$SECRET"
        ;;
    retrieve)
        TITEL=$2
        BEMERKUNG=$3
        ./genkey secGet "$TITEL" "$BEMERKUNG"  | grep -e "^SEC" | cut -c 4-
        ;;
    list)
        ./genkey secList  | grep "^SEC" | cut -c 4-
        ;;
    *)
        echo "store TITEL BEMERKUNG SECRET | retrieve TITEL BEMERKUNG | list"
esac


# hugs genkey.hs <<EOF  | grep -Ei "Main> [-]*[0-9]*" | grep -o "[-]*[0-9]*"



