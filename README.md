# AMath

## AMath.BigDecimal
## Speed and accuracy tests
### Constants:

- <img src="https://render.githubusercontent.com/render/math?math=\pi = 3.1415926535897932384626433832795028841971693993751058209749445923078164062862089986280348253421170679821480865132823">
- <img src="https://render.githubusercontent.com/render/math?math=e = 2.7182818284590452353602874713526624977572470936999595749669676277240766303535475945713821785251664274274663919320030">
- <img src="https://render.githubusercontent.com/render/math?math=\phi = 1.6180339887498948482045868343656381177203091798057628621354486227052604628189024497072072041893911374847540880753868">

### Functions:
- <img src="https://render.githubusercontent.com/render/math?math=x = e^e = 15.15426224147926418976043027262991190552854853685613976914074640591483097373093443260845">

| Function name |                                           Value                                           | Average time |
| :------------ | :---------------------------------------------------------------------------------------: | :----------: |
| Ln(x)         | 2.718281828459045235360287471352662497757247093699959574966967627724076630353547594571382 |   1,016ms    |
| Ln(Ln(x))     | 1.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 |   2,033ms    |
| Exp(x)        | 3814279.104760220592209219594098203571023940536226666075526704125804768896712599661001078 |   1,350ms    |
| Pow(x, x)     | 776486517915808457.3826270721448011126981373874089373336109802377656299833887469648179258 |   3,786ms    |

- <img src="https://render.githubusercontent.com/render/math?math=x = \cfrac\pi4 = 0.785398163397448309615660845819875721049292349843776455243736148076954101571552249657008">

| Function name  |                                           Value                                           | Average time |
| :------------- | :---------------------------------------------------------------------------------------: | :----------: |
| Sin(x)         | 0.707106781186547524400844362104849039284835937688474036588339868995366239231053519425193 |   0,200ms    |
| Cos(x)         | 0.707106781186547524400844362104849039284835937688474036588339868995366239231053519425193 |   0,199ms    |
| Tg(x)          | 1.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 |   0,401ms    |
| Ctg(x)         | 1.000000000000000000000000000000000000000000000000000000000000000000000000000000000000000 |   0,401ms    |
| Arcsin(sin(x)) | 0.785398163397448309615660845819875721049292349843776455243736148076954101571552249657008 |   5,110ms    |
| Arccos(cos(x)) | 0.785398163397448309615660845819875721049292349843776455243736148076954101571552249657008 |   5,529ms    |

### Convertation:
|  Convertation: |                  System.Decimal |                AMath.BigDecimal |                  System.Decimal |
| :------------: | ------------------------------: | ------------------------------: | ------------------------------: |
| System.Decimal |                              -1 |                              -1 |                              -1 |
| System.Decimal |                               0 |                               0 |                               0 |
| System.Decimal |                               1 |                               1 |                               1 |
| System.Decimal |  -79228162514264337593543950335 |  -79228162514264337593543950335 |  -79228162514264337593543950335 |
| System.Decimal |   79228162514264337593543950335 |   79228162514264337593543950335 |   79228162514264337593543950335 |
| System.Decimal | -0,0000000000000000000000000001 | -0.0000000000000000000000000001 | -0,0000000000000000000000000001 |
| System.Decimal |  0,0000000000000000000000000001 |  0.0000000000000000000000000001 |  0,0000000000000000000000000001 |

| Convertation: | System.Double | AMath.BigDecimal | System.Double |
| :-----------: | ------------: | ---------------: | ------------: |
| System.Double |            -1 |               -1 |            -1 |
| System.Double |             0 |                0 |             0 |
| System.Double |             1 |                1 |             1 |
| System.Double |       -5E-324 |                0 |             0 |
| System.Double |        5E-324 |                0 |             0 |

## Work in progress
## TODO:
- [ ] \(optional) BigNatural #*byte array or something*
- [ ] \(optional) BigInteger *BigNatural with sign and some more math operations*
- [ ] \(curently working) BigRational *numerator and denominator as BigIntegers*
- [x] BigDecimal *BigInteger and decimal scale*
- [ ] \(optional) BigFloat *BigInteger and bit scale*
- [ ] BigReal *dynamic and support $???$ and NaN*
- [ ] \(optional) BigComplex *2 BigReal numbers and imaginary*
- [ ] \(optional) BigQuaternion *4 BigReal numbers and imaginary*


*antyrtert*
