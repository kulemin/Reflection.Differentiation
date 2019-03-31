using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Reflection.Differentiation
{
    public static class Algebra
    {
        public static Expression<Func<double, double>> Differentiate(Expression<Func<double, double>> expression)
        {
            var body = expression.Body;
            var parameters = expression.Parameters;
            return Expression.Lambda<Func<double, double>>(DerivativeCreate(body), parameters);
        }

        private static Expression DerivativeCreate(Expression expression)
        {
            if (expression is ConstantExpression)
                return Expression.Constant(0d);
            if (expression is ParameterExpression)
                return Expression.Constant(1d);

            if (expression is BinaryExpression)
            {
                var left = (expression as BinaryExpression).Left;
                var right = (expression as BinaryExpression).Right;

                if (expression.NodeType == ExpressionType.Add)
                    return Expression.Add(DerivativeCreate(left), DerivativeCreate(right));
                if (expression.NodeType == ExpressionType.Subtract)
                    return Expression.Subtract(DerivativeCreate(left), DerivativeCreate(right));
                if (expression.NodeType == ExpressionType.Multiply)
                    return Expression.Add(
                        Expression.Multiply(DerivativeCreate(left), right),
                        Expression.Multiply(DerivativeCreate(right), left));
                if (expression.NodeType == ExpressionType.Divide)
                    return Expression.Divide(
                        Expression.Subtract(
                            Expression.Multiply(DerivativeCreate(left), right),
                            Expression.Multiply(DerivativeCreate(right), left)),
                        Expression.Multiply(right, right));
            }

            if (expression is MethodCallExpression)
            {
                var trigonometry = (expression as MethodCallExpression);


                if (trigonometry.Method.Name == "Sin")
                    return Expression.Multiply(
                        Expression.Call(typeof(Math).GetMethod("Cos"), trigonometry.Arguments[0]),
                        DerivativeCreate(trigonometry.Arguments[0]));

                if (trigonometry.Method.Name == "Cos")
                    return Expression.Negate(Expression.Multiply(
                        Expression.Call(typeof(Math).GetMethod("Sin"), trigonometry.Arguments[0]),
                        DerivativeCreate(trigonometry.Arguments[0])));

                if (trigonometry.Method.Name == "Tan")
                    return Expression.Divide(
                        Expression.Constant(1d),
                            Expression.Multiply(
                                Expression.Multiply(
                                    Expression.Call(typeof(Math).GetMethod("Cos"), trigonometry.Arguments[0]),
                                    DerivativeCreate(trigonometry.Arguments[0])),
                                Expression.Multiply(
                            Expression.Call(typeof(Math).GetMethod("Cos"), trigonometry.Arguments[0]),
                        DerivativeCreate(trigonometry.Arguments[0]))));
            }

            throw new ArgumentException();
        }
    }
}