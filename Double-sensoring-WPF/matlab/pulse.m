function [ meanPulse ] = pulse(colorList1, colorList2, colorList3)
% Ber�knar och plottar medelpulsen �ver n�got tidsintervall.
% Tidsintervallet best�ms av indata till funktionen
fh=findall(0,'type','subplot(2,1,1)');
for i=1:length(fh)
     clo(fh(i));
end

colorList5 = colorList3;
colorList5(:,:,2) = colorList2;
colorList5(:,:,3) = colorList1;

X1 = colorList1-colorList2;
X2 = colorList1+colorList2-2*colorList3;
X1=X1 - mean(X1);
X2=X2 - mean(X2);
X2=std(X1)/std(X2) * X2;

HB=X1-X2;
HB=HB/std(HB);

colorList5(:,:,4) = HB;

puls1=colorList5;
save puls1.mat

for i = 1:4
    colorList = colorList5(:,:,i);
% Inst�llningar; v�lj dina inst�llnignar f�r koden h�r
samplesPerSecPulse = 30;
% Slut inst�llningar

d = fdesign.bandpass('N,F3dB1,F3dB2', 10, 30/60, 100/60, 30);
hd = design(d,'butter');
filtcolorList = filtfilt(hd.sosMatrix,hd.ScaleValues,colorList);



%% Info om m�tdata
numberOfSamplesPulse = length(colorList);
timeOfMeasurementPulse = numberOfSamplesPulse/samplesPerSecPulse;
% Slut p� info om m�tdata

% % G�r l�ngden p� vectorns indata udda
% if (mod(numberOfSamplesPulse,2) == 0) % om colorList �r j�mn i l�ngd
%     colorList = colorList(2:numberOfSamplesPulse);
%     numberOfSamplesPulse = length(colorList);
% end
% % Slut

%% Kontrollerar att listan �r tillr�ckligt l�ng
if length(colorList)<4
    error('Not enough samples of pulse')
end
%Slut kontroll
% 
% %% Filtrera colorList med hj�lp av h�gsta m�jliga gradens
% % Savitzky Golay FIR-filter
% colorList=double(colorList); % Kanske ej beh�vs
% degreeOfPolynomialPulse = samplesPerSecPulse - 1;
% smoothColorList = sgolayfilt(colorList,degreeOfPolynomialPulse,numberOfSamplesPulse);

% Lokaliserar peakarna (topparna) i den filtrerade kurvan
[heightOfPeaksPulse, peakLocationPulse]=findpeaks(filtcolorList);
numberOfPeaksPulse=length(peakLocationPulse);
% Slut filtrering

%% Pulsens medelv�rde �ver antal sekunder
bpmPulse = (numberOfPeaksPulse/timeOfMeasurementPulse)*60;
% Slut medelv�rde �ver antal sekunder

% Utskrifter
meanPulse = round(bpmPulse);
% Slut utskrifter

if (i == 1)
    color = 'red';
elseif (i == 2)
    color = 'green';
elseif (i == 4)
    color = 'black';
else
    color = 'blue';
end

colorList55(:,:,i) = filtcolorList;
if (i == 4)
% Plot av filtrerad data
h = figure(1)
subplot(2,1,1)
%hold off
plot(filtcolorList, color);
hold on
plot(peakLocationPulse, heightOfPeaksPulse, 'black o');
grid on
title({'BPM';meanPulse}, 'color', 'red', 'FontWeight', 'bold')
hold all
xlabel('tid [s/30]')
ylabel('Pulskurva')
% Slut plot av filtrerad data
end
end
hold off
saveas(h, 'pulseplot.png')
end


