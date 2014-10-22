import random
from PIL import Image, ImageDraw

noiseWidth = 512
noiseHeight = 512

noise = [[0 for i in range(noiseWidth)] for j in range (noiseHeight)]

img = Image.new( 'RGB', (noiseWidth,noiseHeight), "white")
pixels = img.load()

def smoothNoise(x, y):
    fractX = x - int(x)
    fractY = y - int(y)

    x1 = (int(x) + noiseWidth) % noiseWidth
    y1 = (int(y) + noiseHeight) % noiseHeight

    x2 = (x1 + noiseWidth - 1) % noiseWidth
    y2 = (y1 + noiseHeight - 1) % noiseHeight

    value = 0.0
    value += fractX * fractY * noise[x1][y1]
    value += fractX * (1 - fractY) * noise[x1][y2]
    value += (1 - fractX) * fractY * noise[x2][y1]
    value += (1 - fractX) * (1 - fractY) * noise[x2][y2]

    return value

def turbulence(x, y, size):
    value = 0.0
    initialSize = size

    while (size >= 1):
        value += smoothNoise(x / size, y / size) * size
        size /= 2.0

    return (128.0 * value / initialSize)

def generateNoise():
    for x in range(0, noiseHeight):
        for y in range(0, noiseWidth):
            noise[x][y] = (random.randrange(0, 32768)) / 32768.0

def main():
    #open output file here

    generateNoise()

    for x in range(0, noiseWidth):
        for y in range(0, noiseHeight):
            t = int(turbulence(x,y,32))
            pixels[x,y] = (t,t,t)

    img.save("out.png", "PNG")

main()


