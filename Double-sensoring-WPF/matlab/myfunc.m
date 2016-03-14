function [x] = myfunc(a) 
%x = plot(a);

%Spara en bra mätning
matning3 = a;
save matning3.mat
figure(1)
subplot(2,2,2)
plot(a);
x=plot(a)


% %KASS KOD
% color = 'none';
% if (max(a)-min(a)) >= 0.05
% 	color = 'red';
% 
% %text(100, float(mean(a)), 'Subject is moving, waiting for stabilizing', 'Color', color, 'Fontsize', 32);
% 
% title('Plot av "Spine"-djup')
% xlabel('Tid/"frames"'), ylabel('Djupvärden [m]');
% meanvalue = mean(a);
% ylim([min(a)-0.05 max(a)+0.05]);
% xlim([0 300]);
% 
% %answer_vector=breathing_instant(a);
% %x=answer_vector;
% 

end