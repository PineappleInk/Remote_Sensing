function [x] = myfunc(a) 
x = plot(a);

%KASS KOD
color = 'green';
%if (max(a)-min(a)) >= 0.01
%	color = 'red';
%
%text(100, mean(a), 'Subject is moving, waiting for stabilizing', 'Color', color, 'Fontsize', 16);
%SLUT PÅ KASS KOD

title('Plot av "Spine"-djup')
xlabel('Tid/"frames"'), ylabel('Djupvärden [m]');
meanvalue = mean(a);
ylim([min(a)-0.05 max(a)+0.05]);
xlim([0 300]);




end