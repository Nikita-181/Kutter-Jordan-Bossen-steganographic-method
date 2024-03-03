# Kutter-Jordan-Bossen steganographic method
This C# solution implements the Kutter-Jordan-Bosson steganographic method (Cross Method), which is used to embed information in an image.

![interface](interface.png)

# Description of the method
This method uses the property of the human visual system - weak sensitivity to slight changes in the brightness of blue color.

The method is based on modifying the blue brightness of individual pixels.

## Embedding
Let $m_i$ be a single bit of the *I = {R, G, B}* message embedded in the image, where *R*, *G* and *B* are the red, green and blue components of the pixel, respectively, and *p = ( x, y)* - pseudorandom coordinate of the pixel into which the embedding will be performed.

The $m_i$ bit is embedded in the blue channel by modifying the brightness:

$$L_{x,y} = 0.29890R_{x,y} + 0.58662G_{x,y} + 0.11448B_{x,y}$$

The embedding is bitwise: one message bit into one blue brightness value $B_{x,y}$. In this case, the modified blue brightness values $B^*_{x,y}$ are calculated using the formula:

$$
B^*_{x,y}=
\begin{cases}
B_{x,y} + λL_{x,y}, & \quad \text{$m_{i}=1$}\\ 
B_{x,y} - λL_{x,y}, & \quad \text{$m_{i}=0$}
\end{cases}
$$

where λ is a constant defining the "energy" of the embedded bit, i.e. it specifies the fraction of full brightness by which the blue color channel is modified. As a rule, λ takes the value 0.01...0.1. The larger λ is, the higher is the robustness (stability) of the embedding, but the stronger is its noticeability.

The pixel coordinates used for embedding are kept secret, i.e. are specified with a secret key. The coordinates can be chosen arbitrarily, taking into account the forecast area. The centers of the crosses must not fall within the prediction regions of other embedded bits. Otherwise, the predicted value may be very different from the true value, resulting in an extraction error.

## Extraction
Extracting a bit of information by the receiver is performed without the presence of the original image. In order to restore the original bit, you need to predict its value.

For realistic images, adjacent pixels always have very close brightness values, that is, the image is usually highly correlated (similar). This makes it possible to calculate a certain predicted blue brightness value $\tilde B_{x,y}$.

That is, on the receiving side, an authorized user who knows the coordinates $x,y$ calculates the predicted value using the "cross" rule.

$$\tilde B_{x,y} = \dfrac{\sum_{i=1}^σ( B_{x+i,y} + B_{x-i,y} + B_{x,y+i} + B_{x,y-i} )}{4σ} $$

where σ is the number of pixels on the top/bottom/left/right of the estimated pixel.

Example of a 5x5 cross, σ = 2:
<p align="center">
  <img src="https://habrastorage.org/r/w1560/storage/habraeffect/5e/3f/5e3ffa869a2b1e001823fc9592b17d86.png">
</p>

Bit extraction is carried out by rule:

$$
m_{i}=
\begin{cases}
B^*_{x,y} > \tilde B_{x,y}, & \quad \text{$1$}\\ 
B^*_{x,y} < \tilde B_{x,y}, & \quad \text{$0$}
\end{cases}
$$

## Multiple Embedding
To reduce the probability of extraction errors during the embedding process, each bit of the message is repeated several times (multiple embedding). Since each bit has been repeated *τ* times, this produces *τ* estimates of a single message bit. The secret bit is extracted by averaging the difference between the real and estimated pixel intensity values in the resulting container:

$$ δ = τ^{-1} \sum_{i=1}^τ [B^*_{x,y} - \tilde B_{x,y}] $$ 

Then the extraction of the secret bit is described as follows:

$$
m_{i}=
\begin{cases}
δ > 0, & \quad \text{$1$}\\ 
δ < 0, & \quad \text{$0$}
\end{cases}
$$

# References
*  Метод Куттера-Джордана-Боссона (Метод «креста») /  [Электронный ресурс] // StudFiles : [сайт]. — URL: https://studfile.net/preview/7736499/page:8/ (дата обращения: 03.03.2024).
* Метод Куттера-Джордана-Боссена /  [Электронный ресурс] // Википедия : [сайт]. — URL: https://ru.wikipedia.org/wiki/%D0%9C%D0%B5%D1%82%D0%BE%D0%B4_%D0%9A%D1%83%D1%82%D1%82%D0%B5%D1%80%D0%B0-%D0%94%D0%B6%D0%BE%D1%80%D0%B4%D0%B0%D0%BD%D0%B0-%D0%91%D0%BE%D1%81%D1%81%D0%B5%D0%BD%D0%B0#:~:text=%D0%9C%D0%B5%D1%82%D0%BE%D0%B4%20%D0%9A%D1%83%D1%82%D1%82%D0%B5%D1%80%D0%B0%2D%D0%94%D0%B6%D0%BE%D1%80%D0%B4%D0%B0%D0%BD%D0%B0%2D%D0%91%D0%BE%D1%81%D1%81%D0%B5%D0%BD%D0%B0%20%2D,Imaging%20%D0%B2%20%D0%B0%D0%BF%D1%80%D0%B5%D0%BB%D0%B5%201998%20%D0%B3%D0%BE%D0%B4%D0%B0. (дата обращения: 03.03.2024).
*  Метод Куттера-Джордана-Боссена /  [Электронный ресурс] // StudFile : [сайт]. — URL: https://studfile.net/preview/7379018/page:33/ (дата обращения: 03.03.2024).