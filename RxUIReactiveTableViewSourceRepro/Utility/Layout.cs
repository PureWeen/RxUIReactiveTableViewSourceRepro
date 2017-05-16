namespace RxUIReactiveTableViewSourceRepro.Utility
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Foundation;
    using ObjCRuntime;
    using UIKit;

    public static class Layout
    {
        // the standard spacing between sibling views
        public const int StandardSiblingViewSpacing = 8;

        // half the standard spacing between sibling views
        public const int HalfSiblingViewSpacing = StandardSiblingViewSpacing / 2;

        // the standard spacing between a view and its superview
        public const int StandardSuperviewSpacing = 20;

        // half the standard spacing between superviews
        public const int HalfSuperviewSpacing = StandardSuperviewSpacing / 2;

        public const float RequiredPriority = (float)UILayoutPriority.Required;

        public const float HighPriority = (float)UILayoutPriority.DefaultHigh;

        public const float LowPriority = (float)UILayoutPriority.DefaultLow;

#if DEBUG

        internal static readonly IDictionary<string, string> constraintSubstitutions = new Dictionary<string, string>();

#endif

        public static void ConstrainLayout(this UIView view, Expression<Func<bool>> constraintsExpression, float priority = RequiredPriority)
        {
            var body = constraintsExpression.Body;
            var constraints = FindBinaryExpressionsRecursive(body)
                .Select(e =>
                {
#if DEBUG

                    if (ExtractAndRegisterName(e, view))
                    {
                        return null;
                    }

#endif

                    return CompileConstraint(e, view, priority);
                })
                .Where(x => x != null)
                .ToArray();

            view.AddConstraints(constraints);
        }

        private static IEnumerable<BinaryExpression> FindBinaryExpressionsRecursive(Expression expression)
        {
            var binaryExpression = expression as BinaryExpression;

            if (binaryExpression == null)
            {
                yield break;
            }

            if (binaryExpression.NodeType == ExpressionType.AndAlso)
            {
                foreach (var childBinaryExpression in FindBinaryExpressionsRecursive(binaryExpression.Left))
                {
                    yield return childBinaryExpression;
                }

                foreach (var childBinaryExpression in FindBinaryExpressionsRecursive(binaryExpression.Right))
                {
                    yield return childBinaryExpression;
                }
            }
            else
            {
                yield return binaryExpression;
            }
        }

#if DEBUG

        // special case to extract names from the expression, such as this.someControl.Name() == nameof(someControl)
        private static bool ExtractAndRegisterName(BinaryExpression binaryExpression, UIView constrainedView)
        {
            if (binaryExpression.NodeType != ExpressionType.Equal)
            {
                return false;
            }

            MethodCallExpression methodCallExpression;
            UIView view;
            NSLayoutAttribute layoutAttribute;
            DetermineConstraintInformationFromExpression(binaryExpression.Left, out methodCallExpression, out view, out layoutAttribute, false);

            if (methodCallExpression == null || methodCallExpression.Method.Name != nameof(LayoutExtensions.Name))
            {
                return false;
            }

            if (binaryExpression.Right.NodeType != ExpressionType.Constant)
            {
                throw new NotSupportedException("When assigning a name to a control, only constants are supported.");
            }

            var name = (string)((ConstantExpression)binaryExpression.Right).Value;
            var iOSName = view.Class.Name + ":0x" + view.Handle.ToString("x");
            constraintSubstitutions[iOSName] = name;

            return true;
        }

#endif

        private static NSLayoutConstraint CompileConstraint(BinaryExpression binaryExpression, UIView constrainedView, float priority)
        {
            NSLayoutRelation layoutRelation;

            switch (binaryExpression.NodeType)
            {
                case ExpressionType.Equal:
                    layoutRelation = NSLayoutRelation.Equal;
                    break;
                case ExpressionType.LessThanOrEqual:
                    layoutRelation = NSLayoutRelation.LessThanOrEqual;
                    break;
                case ExpressionType.GreaterThanOrEqual:
                    layoutRelation = NSLayoutRelation.GreaterThanOrEqual;
                    break;
                default:
                    throw new NotSupportedException("Not a valid relationship for a constraint: " + binaryExpression.NodeType);
            }

            MethodCallExpression methodCallExpression;
            UIView leftView;
            NSLayoutAttribute leftLayoutAttribute;
            DetermineConstraintInformationFromExpression(binaryExpression.Left, out methodCallExpression, out leftView, out leftLayoutAttribute);

            if (leftView != null && leftView != constrainedView)
            {
                leftView.TranslatesAutoresizingMaskIntoConstraints = false;
            }

            UIView rightView;
            NSLayoutAttribute rightLayoutAttribute;
            float multiplier;
            float constant;
            DetermineConstraintInformationFromExpression(binaryExpression.Right, out rightView, out rightLayoutAttribute, out multiplier, out constant);

            if (rightView != null && rightView != constrainedView)
            {
                rightView.TranslatesAutoresizingMaskIntoConstraints = false;
            }

            var constraint = NSLayoutConstraint.Create(
                leftView,
                leftLayoutAttribute,
                layoutRelation,
                rightView,
                rightLayoutAttribute,
                multiplier,
                constant);
            constraint.Priority = priority;
            return constraint;
        }

        private static void DetermineConstraintInformationFromExpression(
            Expression expression,
            out MethodCallExpression methodCallExpression,
            out UIView view,
            out NSLayoutAttribute layoutAttribute,
            bool throwOnError = true)
        {
            methodCallExpression = FindExpressionOfType<MethodCallExpression>(expression);

            if (methodCallExpression == null)
            {
                if (throwOnError)
                {
                    throw new NotSupportedException("Constraint expression must be a method call.");
                }
                else
                {
                    view = null;
                    layoutAttribute = default(NSLayoutAttribute);
                    return;
                }
            }

            layoutAttribute = NSLayoutAttribute.NoAttribute;

            switch (methodCallExpression.Method.Name)
            {
                case nameof(LayoutExtensions.Width):
                    layoutAttribute = NSLayoutAttribute.Width;
                    break;
                case nameof(LayoutExtensions.Height):
                    layoutAttribute = NSLayoutAttribute.Height;
                    break;
                case nameof(LayoutExtensions.Left):
                case nameof(LayoutExtensions.X):
                    layoutAttribute = NSLayoutAttribute.Left;
                    break;
                case nameof(LayoutExtensions.Top):
                case nameof(LayoutExtensions.Y):
                    layoutAttribute = NSLayoutAttribute.Top;
                    break;
                case nameof(LayoutExtensions.Right):
                    layoutAttribute = NSLayoutAttribute.Right;
                    break;
                case nameof(LayoutExtensions.Bottom):
                    layoutAttribute = NSLayoutAttribute.Bottom;
                    break;
                case nameof(LayoutExtensions.CenterX):
                    layoutAttribute = NSLayoutAttribute.CenterX;
                    break;
                case nameof(LayoutExtensions.CenterY):
                    layoutAttribute = NSLayoutAttribute.CenterY;
                    break;
                case nameof(LayoutExtensions.Baseline):
                    layoutAttribute = NSLayoutAttribute.Baseline;
                    break;
                case nameof(LayoutExtensions.Leading):
                    layoutAttribute = NSLayoutAttribute.Leading;
                    break;
                case nameof(LayoutExtensions.Trailing):
                    layoutAttribute = NSLayoutAttribute.Trailing;
                    break;
                default:
                    if (throwOnError)
                    {
                        throw new NotSupportedException("Method call '" + methodCallExpression.Method.Name + "' is not recognized as a valid constraint.");
                    }
                    break;
            }

            if (methodCallExpression.Arguments.Count != 1)
            {
                if (throwOnError)
                {
                    throw new NotSupportedException("Method call '" + methodCallExpression.Method.Name + "' has " + methodCallExpression.Arguments.Count + " arguments, where only 1 is allowed.");
                }
                else
                {
                    view = null;
                    return;
                }
            }

            var viewExpression = methodCallExpression.Arguments.FirstOrDefault() as MemberExpression;

            if (viewExpression == null)
            {
                if (throwOnError)
                {
                    throw new NotSupportedException("The argument to method call '" + methodCallExpression.Method.Name + "' must be a member expression that resolves to the view being constrained.");
                }
                else
                {
                    view = null;
                    return;
                }
            }

            view = Evaluate<UIView>(viewExpression);

            if (view == null)
            {
                if (throwOnError)
                {
                    throw new NotSupportedException("The argument to method call '" + methodCallExpression.Method.Name + "' resolved to null, so the view to be constrained could not be determined.");
                }
                else
                {
                    view = null;
                    return;
                }
            }
        }

        private static void DetermineConstraintInformationFromExpression(
            Expression expression,
            out UIView view,
            out NSLayoutAttribute layoutAttribute,
            out float multiplier,
            out float constant)
        {
            var viewExpression = expression;

            view = null;
            layoutAttribute = NSLayoutAttribute.NoAttribute;
            multiplier = 1.0f;
            constant = 0.0f;

            if (viewExpression.NodeType == ExpressionType.Add || viewExpression.NodeType == ExpressionType.Subtract)
            {
                var binaryExpression = (BinaryExpression)viewExpression;
                constant = Evaluate<float>(binaryExpression.Right);

                if (viewExpression.NodeType == ExpressionType.Subtract)
                {
                    constant = -constant;
                }

                viewExpression = binaryExpression.Left;
            }

            if (viewExpression.NodeType == ExpressionType.Multiply || viewExpression.NodeType == ExpressionType.Divide)
            {
                var binaryExpression = (BinaryExpression)viewExpression;
                multiplier = Evaluate<float>(binaryExpression.Right);

                if (viewExpression.NodeType == ExpressionType.Divide)
                {
                    multiplier = 1 / multiplier;
                }

                viewExpression = binaryExpression.Left;
            }

            if (viewExpression is MethodCallExpression)
            {
                MethodCallExpression methodCallExpression;
                DetermineConstraintInformationFromExpression(viewExpression, out methodCallExpression, out view, out layoutAttribute);
            }
            else
            {
                // constraint must be something like: view.Width() == 50
                constant = Evaluate<float>(viewExpression);
            }
        }

        private static T Evaluate<T>(Expression expression)
        {
            var result = Evaluate(expression);

            if (result is T)
            {
                return (T)result;
            }

            return (T)Convert.ChangeType(Evaluate(expression), typeof(T));
        }

        private static object Evaluate(Expression expression)
        {
            if (expression.NodeType == ExpressionType.Constant)
            {
                return ((ConstantExpression)expression).Value;
            }

            if (expression.NodeType == ExpressionType.MemberAccess)
            {
                var memberExpression = (MemberExpression)expression;
                var member = memberExpression.Member;

                if (member.MemberType == MemberTypes.Field)
                {
                    var fieldInfo = (FieldInfo)member;

                    if (fieldInfo.IsStatic)
                    {
                        return fieldInfo.GetValue(null);
                    }
                }
            }

            return Expression.Lambda(expression).Compile().DynamicInvoke();
        }

        // searches for an expression of type T within expression, skipping through "irrelevant" nodes
        private static T FindExpressionOfType<T>(Expression expression)
            where T : Expression
        {
            while (!(expression is T))
            {
                switch (expression.NodeType)
                {
                    case ExpressionType.Convert:
                        expression = ((UnaryExpression)expression).Operand;
                        break;
                    default:
                        return default(T);
                }
            }

            return (T)expression;
        }

#if DEBUG

        public static class DebugConstraint
        {
            private delegate IntPtr DescriptionDelegate(IntPtr self, IntPtr sel);
            private static DescriptionDelegate replacementDescriptionImplementation = new DescriptionDelegate(Description);

            public static void Swizzle()
            {
                var constraintClass = Class.GetHandle(typeof(NSLayoutConstraint));
                var method = class_getInstanceMethod(constraintClass, Selector.GetHandle("description"));
                var originalImpl = class_getMethodImplementation(constraintClass, Selector.GetHandle("description"));

                // add the original implementation to respond to 'customDescription'
                class_addMethod(constraintClass, Selector.GetHandle("customDescription"), originalImpl, "@@:");

                // replace the original implementation with our own for the 'descriptor' method.
                var newImpl = System.Runtime.InteropServices.Marshal.GetFunctionPointerForDelegate(replacementDescriptionImplementation);
                method_setImplementation(method, newImpl);
            }

            [ObjCRuntime.MonoPInvokeCallback(typeof(DescriptionDelegate))]
            public static IntPtr Description(IntPtr self, IntPtr sel)
            {
                var originalDescriptionPtr = objc_msgSend(self, Selector.GetHandle("customDescription"));
                var originalDescription = Runtime.GetNSObject<NSString>(originalDescriptionPtr);
                var description = originalDescription.ToString();

                foreach (var substitution in Layout.constraintSubstitutions)
                {
                    description = description.Replace(substitution.Key, substitution.Value);
                }

                return new NSString(description).Handle;
            }

            [System.Runtime.InteropServices.DllImport("libobjc.dylib")]
            static extern IntPtr objc_msgSend(IntPtr handle, IntPtr sel);

            [System.Runtime.InteropServices.DllImport("libobjc.dylib")]
            static extern IntPtr class_getInstanceMethod(IntPtr c, IntPtr sel);

            [System.Runtime.InteropServices.DllImport("libobjc.dylib")]
            static extern bool class_addMethod(IntPtr cls, IntPtr name, IntPtr imp, string types);

            [System.Runtime.InteropServices.DllImport("libobjc.dylib")]
            extern static IntPtr class_getMethodImplementation(IntPtr cls, IntPtr sel);

            [System.Runtime.InteropServices.DllImport("libobjc.dylib")]
            extern static IntPtr method_setImplementation(IntPtr method, IntPtr imp);
        }

#endif
    }
}