   M S B A M L   `   `   `  ����  CAGA%    CAGA.Dialogue.StatResultsWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35` \RibbonControlsLibrary, Version=4.0.0.11019, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Selection Statistics��5   	   $	��360��6&   $	��480��63    � WindowStartupLocation$ CenterScreen=�6?    � SizeToContent$ WidthAndHeight=�6d    � WindowStyle$ 
ToolWindow=�6�    � 	Resources 5         HoveredItemBrush �� EndPoint �� 
StartPoint � Color � Offset  StatListTemplate	 �� DataType
 ��Name �� Padding �� CornerRadius �� Triggers �� Value �� Binding �� RelativeSource �� Mode �� AncestorType �� Path �� 
TargetName �� Value �� Property��%'  &	        &	 �     7   7   �� 5      $ 0.5,1&�63   $ 0.5,0&�6D   .��5   
   � $ Lavender��6   $
 0.15Q�6(   � 5	   
   $ 	AliceBlue��6   $
 0.85Q�6)   � 5
   
   $ 	CadetBlue��6   $ 1Q�6)   5      �� 5      $	 Whatever��6,   .R�5   
   �� $
 	TheBorder��6   
�������$��1q�67   $ 5q�6K   $ 4��6W   $��5q�6h   ._�5      � .+�5      �� $��80��61   $
��Bold"�6<   ��6   �� *Key+6P   �� 5      $	��100��63   ��6   �� *Value+6A   5      ��5      �� 5      $ 0.5,1&�6&   $ 0.5,0&�67   .��5      � $ 	AliceBlue��6#   $
 0.15Q�65   � 5      $ White��6#   $
 0.85Q�61   � 5      $ Lavender��6#   $ 1Q�64   5      5       5   
   �� 5      $
 True��6�    6   ��  �� $ FindAncestor=�" ��$ IsMouseOver�.a�5      �� $ 	TheBorder��6   8    � ��6F   5      �� 5      $
 True��69    6   �� *
IsSelected+.a�5      �� $ 	TheBorder��6   8   � ��6F   5       5#      5$      .��5&      � $��Center=�6   $��Center=�6(   $��10q�6C    � ColumnDefinitions 5'   
   �� 5(      $
��auto �6   �� 5)      $
��auto �6   .+�5+   
   �� $��0��6   .+�5,      � $��5q�6   .+�5-      �� $��Layer: ��6   $
��Bold"�6*   �� 5.      -   5        $��LayerNameTB��5.   $   $�� ��6   $��Center=�67   6T   5/      � 50      $��5q�6   .+�51      �� $��Field: ��6   $
��Bold"�6*   �� 52      -   5        $��FieldNameTB��52   $   $�� ��6   $��Center=�67   6T   53      �� 54      $��Statistics:��6   $��5q�6*   $��5q�65   $
��Bold"�6A   �� 55      -   5        $��StatisticsLB��55      #��[ 6*   
������ 
������  57        NoFocusVisualStyle �� 
TargetType �� Color��%�  &	       '�� J     7  (�� �     5A   &   * SystemColors.HighlightBrushKey+)(�� �     5C   &   *SystemColors.ControlBrushKey+)(�� �     5E   &   *$"SystemColors.HighlightTextBrushKey+)�� 59      " ��68   .�5:      ��  � ��
 ���� 68   5;      �� 5=      " ��69   .�5>      ��  �@�FocusVisualStyle8   6=   5?      �� 5A      $ Transparent��6X   �� 5C      $ Transparent��6V   �� 5E      $ Black��6\   5F      5G      5H      �� 5I   
   $��1��6   .+�5J      �� -   5        $��	statGraph��5J      6'   �� 5K      -   5        $��AddGraphToLayout��5K      $��Add to Layout��6-   6f   5L      5M      5N      