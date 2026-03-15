#!/bin/sh

VERSION=${VERSION:-latest}
REPO="git+https://github.com/github/spec-kit.git"

if [ "$VERSION" = "latest" ]; then
    uv tool install specify-cli --from "$REPO"
else
    uv tool install specify-cli --from "${REPO}@${VERSION}"
fi
