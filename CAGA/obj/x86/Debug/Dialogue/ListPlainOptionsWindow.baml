   M S B A M L   `   `   `  ����  CAGA*    $CAGA.Dialogue.ListPlainOptionsWindow   5   	   \ XPresentationFramework, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35R NWindowsBase, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35^ ZWindowsFormsIntegration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35` \RibbonControlsLibrary, Version=4.0.0.11019, Culture=neutral, PublicKeyToken=31bf3856ad364e35W SPresentationCore, Version=4.0.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35H 9http://schemas.microsoft.com/winfx/2006/xaml/presentation      5   	   R NSystem.Xaml, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089:x,http://schemas.microsoft.com/winfx/2006/xaml     5   	     � Title$  Choose an option��5   	   $	��360��6"   $	��480��6/    � WindowStartupLocation$ CenterScreen=�6;    � SizeToContent$ WidthAndHeight=�6`    � WindowStyle$ 
ToolWindow=�6    � 	Resources 5         HoveredItemBrush �� EndPoint �� 
StartPoint � Color � Offset  OptionListTemplate	 �� DataType
 ��Name �� Padding �� CornerRadius �� Triggers �� Value �� Binding �� RelativeSource �� Mode �� AncestorType �� Path �� 
TargetName �� Value �� Property��%?  &	        &	 �     7   7   �� 5   
   $ 0.5,1&�67   $ 0.5,0&�6H   .��5      � $ Lavender��6   $
 0.15Q�6,   � 5	      $ 	AliceBlue��6   $
 0.85Q�6-   � 5
      $ 	CadetBlue��6   $ 1Q�6-   5      �� 5   
   $	 Whatever��62   .R�5      �� $
 	TheBorder��6   
�������$��1q�6;   $ 10q�6O   $ 4��6\   $��5q�6m   ._�5      �� .+�5      �� $	��350��67   $��13(�6C   $
��Bold"�6Q   ��6    �� *Title+6e   �� 5      $	��350��6=   ��6    �� *Description+6K   5      ��5      �� 5      $ 0.5,1&�6*   $ 0.5,0&�6;   .��5      � $ 	AliceBlue��6'   $
 0.15Q�69   � 5      $ White��6'   $
 0.85Q�65   � 5      $ Lavender��6'   $ 1Q�68   5      5       5      �� 5      $
 True��6�    6   ��  �� $ FindAncestor=�" ��$ IsMouseOver�.a�5      �� $ 	TheBorder��6   8    � ��6J   5      �� 5      $
 True��6=    6   �� *
IsSelected+.a�5      �� $ 	TheBorder��6   8   � ��6J   5       5"      5#      .��5%      � $��Center=�6   $��Center=�6(   $��10q�6C    � RowDefinitions 5&   
   �� 5'      $��* �6   �� 5(      $��* �6   .+�5*   
   �� -   5        $��	HeadingTB��5*   0   $��0��6   $��Heading��6!   $��5q�6A    �� Padding$ 5q�6L   $��30(�6X   $
��Bold"�6f   $
��Wrap=�6x   6�   �� 5+   
   -   5        $��OptionLB��5+      $��1��6   #��[ 6/   
������ 
������  5,        NoFocusVisualStyle �� 
TargetType �� Color��%�  &	       '�� J     7  (�� �     57   "   * SystemColors.HighlightBrushKey+)(�� �     59   "   *SystemColors.ControlBrushKey+)(�� �     5;   "   *$"SystemColors.HighlightTextBrushKey+)�� 5.      " ��64   .�5/      ��  � ��
 ���� 64   50      �� 52      " ��65   .�53      ��  �@�FocusVisualStyle8   69   -   5        55      �� 57      $ Transparent��6T   �� 59      $ Transparent��6R   �� 5;      $ Black��6X   5=      5?      5@      5A      