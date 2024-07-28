local bulletSource = {}

bulletSource.name = "GooberHelper/BulletSource"
bulletSource.depth = 9000
bulletSource.texture = "objects/door/lockdoor12"
bulletSource.justification = {0.5, 0.5}
bulletSource.placements = {
    name = "BulletSource",
    data = {
        hitboxScale = 1
    }
}

return bulletSource