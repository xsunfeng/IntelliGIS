   M S B A M L   `   `   `  ����  CAGA-    'CAGA.Dialogue.ListMapLayerOptionsWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35` \RibbonControlsLibrary, Version=4.0.0.11019, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Select map layers��5   	   $	��580��6#   $	��700��60    � WindowStartupLocation$ CenterScreen=�6<    � SizeToContent$ WidthAndHeight=�6a    � WindowStyle$ 
ToolWindow=�6�    � 	Resources 5         HoveredItemBrush �� EndPoint �� 
StartPoint � Color � Offset  OptionListTemplate	 �� DataType
 ��Name �� Padding �� CornerRadius �� Triggers �� Value �� Binding �� RelativeSource �� Mode �� AncestorType �� Path �� 
TargetName �� Value �� Property��%  &	        &	 �     7   7   �� 5   
   $ 0.5,1&�67   $ 0.5,0&�6H   .��5      � $ Lavender��6   $
 0.15Q�6,   � 5      $ 	AliceBlue��6   $
 0.85Q�6-   � 5	      $ 	CadetBlue��6   $ 1Q�6-   5
      �� 5   
   $	 Whatever��62   .R�5      �� $
 	TheBorder��6   
�������$��1q�6;   $ 10q�6O   $ 4��6\   $��5q�6m   ._�5      �� .+�5      �� -   5        $	��150��5   =   $��13(�6I   $
��Bold"�6W   ��6   �� *	LayerName+6�   5      ��5      �� 5      $ 0.5,1&�6*   $ 0.5,0&�6;   .��5      � $ 	AliceBlue��6'   $
 0.15Q�69   � 5      $ White��6'   $
 0.85Q�65   � 5      $ Lavender��6'   $ 1Q�68   5      5       5      �� 5      $
 True��6�    6   ��  �� $ FindAncestor=�" ��$ IsMouseOver�.a�5      �� $ 	TheBorder��6   8    � ��6J   5      �� 5      $
 True��6=    6   �� *
IsSelected+.a�5      �� $ 	TheBorder��6   8   � ��6J   5      5!      5"      .��5#      � $��Center=�6   $��Center=�6(   $��10q�6C    � RowDefinitions 5$   
   �� 5%      $
��auto �6   �� 5&      $	��480 �6    � ColumnDefinitions 5(   
   �� 5)      $	��200 �6   �� 5*      $	��480 �6   .+�5,   
   �� -   5        $��	HeadingTB��5,   D   $��0��6   $��2��6!   $��Heading��65   $��5q�6U    �� Padding$ 5q�6`   $��30(�6l   $
��Bold"�6z   $
��Wrap=�6�   6�   �� 5-   
   -   5        $��OptionLB��5-   /   $��1��6   $��0��6   #��[ 6?   
������ 
������  5/        NoFocusVisualStyle �� 
TargetType �� Color��%�  &	       '�� J     7  (�� �     5:   "   * SystemColors.HighlightBrushKey+)(�� �     5<   "   *SystemColors.ControlBrushKey+)(�� �     5>   "   *$"SystemColors.HighlightTextBrushKey+)�� 51      " ��64   .�52      ��  � ��
 ���� 64   53      �� 55      " ��65   .�56      ��  �@�FocusVisualStyle8   69   58      �� 5:      $ Transparent��6T   �� 5<      $ Transparent��6R   �� 5>      $ Black��6X   5@      5B      � 5C   
   -   5        $��mapGrid��5C      $��1��6   $��1��6+   6=   5D      5E      