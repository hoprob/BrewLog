if not exist .\.data mkdir .\.data

docker run --rm -v .\.data:/var/lib/eventstore -p 2113:2113 -p 1113:1113 eventstore/eventstore:22.10.1-buster-slim --insecure --run-projections=All --enable-external-tcp --enable-atom-pub-over-http