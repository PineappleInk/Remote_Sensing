function [ HB ] = RGB_Shadinrakar( R, G, B)
%Shadinrakar et al function for stronger signal using all color channels

X1 = R-G;
X2 = R+G-2*B;
X1=X1 - mean(X1);
X2=X2 - mean(X2);
X2=std(X1)/std(X2) * X2;

HB=X1-X2;
HB=HB/std(HB);
end

